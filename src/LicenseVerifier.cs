/***************************************************************************
 *  LicenseVerifier.cs
 *
 *  Copyright (C) 2006 Luke Hoersten
 *  Licensed under GNU LGPL (http://www.opensource.org/licenses/lgpl-license.php)
 *  Written by Luke Hoersten <luke.hoersten@gmail.com>
 ****************************************************************************/

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
    