#if (DEBUG)
using System;
using System.IO;
using NUnit.Framework;
using System.Security.Cryptography;
using CreativeCommons;

namespace CreativeCommons.Tests
{
	[TestFixture]
	public class Tester
	{
	    [Test]
        public void TranscoderTest ()
        {
            string file_path = "../../tests/test.mp3";
            string file_hash = "73JVU77XMPSSX5TUVEPYGRIQADIX6M4B";
            
            SHA1Managed hasher = new SHA1Managed ();
            string encoded_file_hash = Transcoder.Base32Encode (
                                            hasher.ComputeHash (File.OpenRead (file_path)));
            Assert.AreEqual (encoded_file_hash, file_hash);
        }
        
        [Test]
		public void VerifyGoodLocalFileTest ()
		{
		    string file_path = "../../tests/test.mp3";
		    string license_url = "http://creativecommons.org/licenses/by/2.5/";
		    string metadata_url = "../../tests/test.html";
		    
		    Assert.IsTrue (Verifier.VerifyLicense (file_path, license_url, metadata_url));
		}
		    
		[Test]
		public void VerifyBadLocalFileTest ()
		{
		    string file_path = "../../tests/test.mp3";
		    string license_url = "http://creativecommons.org/licenses/by/2.5/";
		    string metadata_url = "../../tests/test.mp3";
		    
		    Assert.IsFalse (Verifier.VerifyLicense (file_path, license_url, metadata_url));
		}
		    
		[Test]
		public void VerifyGoodHttpFileTest ()
		{
		    string file_path = "../../tests/test.mp3";
		    string license_url = "http://creativecommons.org/licenses/by/2.5/";
		    string metadata_url = "http://www.openradix.org/pub/code/test.html";
		    
		    Assert.IsTrue (Verifier.VerifyLicense (file_path, license_url, new Uri (metadata_url)));
		}
		
		[Test]
		public void VerifyBadHttpFileTest ()
		{
		    string file_path = "../../tests/test.mp3";
		    string license_url = "http://creativecommons.org/licenses/by/2.5/";
		    string metadata_url = "http://www.openradix.org/";
		    
		    Assert.IsFalse (Verifier.VerifyLicense (file_path, license_url, new Uri (metadata_url)));
		}
	}
}
#endif
