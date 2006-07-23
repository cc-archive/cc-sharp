/***************************************************************************
 *  LicenseVerifier.cs
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

namespace CreativeCommons
{
    public class LicenseVerifier
    {
        private class LicenseClaim
        {
            private string file_uri;       // File to verify
            private string license_uri;    // License of file
            private string metadata_uri;   // Verification information
            
            public LicenseClaim (string fileUri, string licenseUri, string metadataUri)
            {
                file_uri = fileUri;
                license_uri = licenseUri;
                metadata_uri = metadataUri;
            }
            
            public string FileUri {
    			get { return file_uri; }
    			set { file_uri = value; }
    		}
    		
    		public string LicenseUri {
    			get { return license_uri; }
    			set { license_uri = value; }
    		}
    		
    		public string MetadataUri {
    			get { return metadata_uri; }
    			set { metadata_uri = value; }
    		}
        }
        
        private string verified_license_uri; // Return value
        
        public string VerifiedLicenseUri {
			get { return verified_license_uri; }		}
        
        public LicenseVerifier (string fileUri, string licenseUri, string metadataUri)
        {
            LicenseClaim claim = new LicenseClaim (fileUri, licenseUri, metadataUri);
            verified_license_uri = VerifyLicenseClaim (claim);
        }
        
        private string VerifyLicenseClaim (LicenseClaim claim)
        {
            RdfExtractor parser = new RdfExtractor (claim.MetadataUri);
            return FindLicenseInMetadata (claim.LicenseUri, claim.FileUri, parser.ExtractRdf ());
        }
        
        public static string FindLicenseInMetadata (string licenseUri, string fileUri, string metadata)
        {
            XPathDocument doc = new XPathDocument (new StringReader (metadata));
            XPathNavigator navigator = doc.CreateNavigator ();
            XPathExpression expression = navigator.Compile (
                String.Format ("/rdf:RDF/r:Work[@rdf:about='urn:sha1:{0}']/r:license[@rdf:resource='{1}']",                                
                HashData (fileUri),
                licenseUri));
            
       	    XmlNamespaceManager namespaces = new XmlNamespaceManager (new NameTable ());
       	    namespaces.AddNamespace ("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
       	    namespaces.AddNamespace ("dc", "http://purl.org/dc/elements/1.1/");
       	    namespaces.AddNamespace ("r", "http://web.resource.org/cc/");
       	    expression.SetContext (namespaces);
       	    
       	    XPathNodeIterator it = navigator.Select (expression);
       	    Console.WriteLine ("Found {0} license match(es) in metadata.", it.Count);
       	    
       	    if (it.Count < 1)
       	        return null;
       	    else
       	        return licenseUri;
        }
    
        public static string HashData (string fileUri)
        {
            SHA1Managed hasher = new SHA1Managed ();
            Base32 b32 = new Base32 (hasher.ComputeHash (File.OpenRead (fileUri)));
            string fileHash = b32.ToString ();
            Console.WriteLine ("File: \"{0}\" Hash: \"{1}\"", fileUri, fileHash);
            return fileHash;
        }
    }
}
    