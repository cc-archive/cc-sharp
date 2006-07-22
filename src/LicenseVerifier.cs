/***************************************************************************
 *  LicenseVerifier.cs
 *
 *  Copyright (C) 2006 Luke Hoersten
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
            
            public LicenseClaim(string file_uri, string license_uri, string metadata_uri)
            {
                this.file_uri = file_uri;
                this.license_uri = license_uri;
                this.metadata_uri = metadata_uri;
            }
            
            public string FileUri
    		{
    			get { return file_uri; }
    			set { file_uri = value; }
    		}
    		
    		public string LicenseUri
    		{
    			get { return license_uri; }
    			set { license_uri = value; }
    		}
    		
    		public string MetadataUri
    		{
    			get { return metadata_uri; }
    			set { metadata_uri = value; }
    		}
        }
        
        private string verified_license_uri;
        
        public string VerifiedLicenseUri
		{
			get { return verified_license_uri; }		}
        
        public LicenseVerifier(string file_uri, string license_uri, string metadata_uri)
        {
            LicenseClaim claim = new LicenseClaim(file_uri, license_uri, metadata_uri);
            verified_license_uri = VerifyLicenseClaim(claim);
        }
        
        private string VerifyLicenseClaim(LicenseClaim claim)
        {
            RdfParser parser = new RdfParser(claim.MetadataUri);
            return FindLicenseInMetadata(claim.LicenseUri, claim.FileUri, parser.ParseRdf());
        }
        
        public static string FindLicenseInMetadata(string license_uri, string file_uri, string metadata)
        {
            XPathDocument doc = new XPathDocument(new StringReader(metadata));
       	    XPathNavigator navigator = doc.CreateNavigator();
       	    XPathExpression expression = navigator.Compile(
                String.Format("/rdf:RDF/r:Work[@rdf:about='urn:sha1:{0}']/r:license/@rdf:resource",
                HashData(file_uri)));

       	    XmlNamespaceManager namespaces = new XmlNamespaceManager(new NameTable());
       	    namespaces.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
       	    namespaces.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
       	    namespaces.AddNamespace("r", "http://web.resource.org/cc/");
       	    expression.SetContext(namespaces);

       	    XPathNodeIterator it = navigator.Select(expression);
       	    Console.WriteLine("Found {0} license match(es) in metadata.", it.Count);
       	    while (it.MoveNext()) {
       	        XPathNavigator n = it.Current;
       	        if(n.Value == license_uri) {
       	            return license_uri;
       	        }
       	    }
            return null;
        }
    
        public static string HashData(string file_uri)
        {
            SHA1Managed hasher = new SHA1Managed();
            Base32 b32 = new Base32(hasher.ComputeHash(File.OpenRead(file_uri)));
            string file_hash = b32.ToString();
            Console.WriteLine("File: \"{0}\" Hash: \"{1}\"", file_uri, file_hash);
            return file_hash;
        }
    }
}
    