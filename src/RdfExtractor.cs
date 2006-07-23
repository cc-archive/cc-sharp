/***************************************************************************
 *  RdfExtractor.cs
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
using System.Net;
using System.IO;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;

namespace CreativeCommons
{
	public class RdfExtractor
	{
	    private Stream stream;
	    		public RdfExtractor (string uri)
		{
		    HttpWebRequest request = (HttpWebRequest) WebRequest.Create (uri);
			HttpWebResponse response = (HttpWebResponse) request.GetResponse ();
			stream = response.GetResponseStream ();
		}

		public RdfExtractor (Stream stream)
		{
			this.stream = stream;
		}

		public string ExtractRdf ()
		{
		    Regex expression = new Regex (@"(\<rdf:RDF xmlns=""http://web.resource.org/cc/""[\s\S]{0,}?\/rdf:RDF\>)");
		    StreamReader reader = new StreamReader (stream);
			MatchCollection matches = expression.Matches (reader.ReadToEnd ());
			
			StringBuilder result = new StringBuilder ();
			foreach (Match line in matches)
				result.Append (line.Value);
			
			return result.ToString ();
		}
			
        public static void Main (string [] args)
        {
            if (args.Length < 1) {
                Console.WriteLine ("Must give name of file(s) to parse Creative Commons RDF metadata from.");
                return;
            }
            
            foreach (string uri in args) {
                RdfExtractor parser = new RdfExtractor (uri);
                Console.WriteLine ("File: \"{0}\" RDF: \"{1}\"", uri, parser.ExtractRdf ());
                Console.WriteLine ("\n==================\n");
            }
        }
	}
}
	
