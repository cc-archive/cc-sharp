/***************************************************************************
 *  Base32.cs
 *
 *  Copyright (C) 2006 Luke Hoersten
 *  Written by Luke Hoersten <luke.hoersten@gmail.com>
 *
 * Encode method based on Java code by Robert Kaye and Gordon Mohr
 * Public Domain (PD) 2006 The Bitzi Corporation (http://bitzi.com/publicdomain)
 * (RFC http://www.faqs.org/rfcs/rfc3548.html)
 ****************************************************************************/
 
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace CreativeCommons
{
    public class Base32
    {
        private const int IN_BYTE_SIZE = 8;
        private const int OUT_BYTE_SIZE = 5;
        private static char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();
        private string base32_string;        
        
        public Base32(byte[] data)
        {
            base32_string = Encode(data);
        }
        
        public static string Encode(byte[] data)
        {
            int i = 0, index = 0, digit = 0;
            int currByte, nextByte;
            StringBuilder result = new StringBuilder((data.Length + 7) * IN_BYTE_SIZE / OUT_BYTE_SIZE);
            while(i < data.Length) {
                currByte = (data[i] >= 0) ? data[i] : (data[i] + 256); // Is unsigning needed?

                /* Is the current digit going to span a byte boundary? */
                if(index > (IN_BYTE_SIZE - OUT_BYTE_SIZE)) {
                    if ((i + 1) < data.Length) {
                        nextByte = (data[i + 1] >= 0) ? data[i + 1] : (data[i + 1] + 256);
                    } else {
                        nextByte = 0;
                    }

                    digit = currByte & (0xFF >> index);
                    index = (index + OUT_BYTE_SIZE) % IN_BYTE_SIZE;
                    digit <<= index;
                    digit |= nextByte >> (IN_BYTE_SIZE - index);
                    i++;
                } else {
                    digit = (currByte >> (IN_BYTE_SIZE - (index + OUT_BYTE_SIZE))) & 0x1F;
                    index = (index + OUT_BYTE_SIZE) % IN_BYTE_SIZE;
                    if (index == 0)
                        i++;
                }
                result.Append(alphabet[digit]);
            }

            return result.ToString();
        }
        
        public override string ToString()
        {
            return base32_string;
        }
        
        public static void Main(string[] args)
        {
            if(args.Length < 1) {
                Console.WriteLine("Must give name of file(s) to be sha1 hashed.");
                return;
            }
            
            SHA1Managed hasher = new SHA1Managed();
            foreach(string file_name in args) {
                Base32 base32 = new Base32(hasher.ComputeHash(File.OpenRead(file_name)));
                Console.WriteLine("File: \"{0}\" Hash: \"{1}\"", file_name, base32.ToString());
            }
        }
    }
}