/***************************************************************************
 *  Verifier.cs
 *
 *  cc-sharp is a library to verify Creative Commons license metadata.
 *  Copyright (C) 2006 Luke Hoersten
 *  Written by Luke Hoersten <luke.hoersten@gmail.com>
 ****************************************************************************/
 
/*  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Security.Cryptography;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CreativeCommons
{
    public static class Verifier
    {
        public static bool VerifyLicense (string licenseUri, string filePath, Uri metadataUrl)
        {
            try {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create (metadataUrl);
                HttpWebResponse response = (HttpWebResponse) request.GetResponse ();
                return LicenseInStream (licenseUri, filePath, response.GetResponseStream ());
            } catch(XmlException e) {
                return false;
            } catch(XPathException e) {
                return false;
            }
        }
        
        public static bool VerifyLicense (string licenseUri, string filePath, string metadataPath)
        {
            try {
                return LicenseInStream (licenseUri, filePath, File.OpenRead (metadataPath));
            } catch(XmlException e) {
                return false;
            } catch(XPathException e) {
                return false;
            }
        }
        
        public static bool LicenseInStream (string licenseUri, string filePath, Stream metadataStream)
        {
            Regex expression = new Regex (@"(\<rdf:RDF xmlns=""http://web.resource.org/cc/""[\s\S]{0,}?\/rdf:RDF\>)");
            StreamReader reader = new StreamReader (metadataStream);
            MatchCollection matches = expression.Matches (reader.ReadToEnd ());

            foreach (Match metadata in matches)
                if (LicenseInMetadata (licenseUri, filePath, metadata.Value))
                    return true;
            	
            return false;
        }
        
        public static string HashFile (string filePath)
        {
            SHA1Managed hasher = new SHA1Managed ();
            string file_hash = Transcoder.Base32Encode (hasher.ComputeHash (File.OpenRead (filePath)));
            Console.WriteLine ("File: \"{0}\" Hash: \"{1}\"", filePath, file_hash);
            return file_hash;
        }
        
        private static bool LicenseInMetadata (string licenseUri, string filePath, string metadata)
        {
            XPathDocument doc = new XPathDocument (new StringReader (metadata));
            XPathNavigator navigator = doc.CreateNavigator ();
            XPathExpression expression = navigator.Compile (
            String.Format ("/rdf:RDF/r:Work[@rdf:about='urn:sha1:{0}']/r:license[@rdf:resource='{1}']",                                
                HashFile (filePath),
                licenseUri));

            XmlNamespaceManager namespaces = new XmlNamespaceManager (new NameTable ());
            namespaces.AddNamespace ("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            namespaces.AddNamespace ("dc", "http://purl.org/dc/elements/1.1/");
            namespaces.AddNamespace ("r", "http://web.resource.org/cc/");
            expression.SetContext (namespaces);

            XPathNodeIterator it = navigator.Select (expression);
            Console.WriteLine ("Found {0} license match(es) in metadata.", it.Count);

            if (it.Count > 0)
                return true;

            return false;
        }
    }
}
    