using System;
using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wiz101Extractor
{
    struct WadHeader
    {
        public int version, entryCount;
    }
    
    
    class WadEntry
    {
        public int offset;
        public int size;
        public int compSize;
        public bool zipped;
        public int checkSum;
        public int path_length;
        public String name;
        public byte[] buffer;

    }
    
    class WizardReader
    {
        List<WadEntry> entries;
        WadHeader header;
        BinaryReader binaryReader;
        
        private void ReadHeader()
        {
            header = new WadHeader();
            binaryReader.ReadBytes(5);
            header.version = binaryReader.ReadInt32();
            header.entryCount = binaryReader.ReadInt32();
            if(header.version == 2)
            {
                binaryReader.ReadByte();
            }
        }

        private void GetEntryBuffers(String filename)
        {

            foreach(WadEntry entry in entries){

                

                FileStream fileStream = File.OpenRead(filename);
                fileStream.Position = entry.offset;
                

                if (entry.zipped)
                {
                    using (DeflateStream decompressor = new DeflateStream(fileStream, CompressionMode.Decompress))
                    {
                        fileStream.Position += 2; // skip zlib header
                        decompressor.Read(entry.buffer, 0, (int)entry.size);
                    }
                }
                else
                {
                    fileStream.Read(entry.buffer, 0, entry.size);               
                }
            }
        }

       
        public WadHeader GetHeader()
        {
            return header;
        }

        public void DumpEntries(String outdir)
        {
            foreach (WadEntry entry in entries)
            {
                
                String directory;
                String filename;

                //scrub null characters
                if (entry.name.Contains("\0"))
                {
                    entry.name = entry.name.Substring(0, Math.Max(0, entry.name.IndexOf('\0')));

                }


                filename = entry.name;
                directory = outdir;

                if (entry.name.Contains("/"))
                {
                    directory = outdir + entry.name.Substring(0, entry.name.LastIndexOf('/'));
                    filename = entry.name.Substring(entry.name.LastIndexOf('/'), entry.name.Length - entry.name.LastIndexOf('/'));
                    
                }
                

                System.IO.Directory.CreateDirectory(directory);
                BinaryWriter output = new BinaryWriter(new FileStream(directory + filename, FileMode.Create));
                output.Write(entry.buffer.ToArray());
                output.Close();


            }
            MessageBox.Show("Finished extracting " + entries.Count + " Wad entries to " + outdir , "BBQGiraffe's Fucking Wizard 101 Extractor has finished extracting");


        }

        private void GetEntries()
        {
            for(int i = 0; i < GetFileCount(); i++)
            {
                WadEntry entry = new WadEntry();
                entry.offset = binaryReader.ReadInt32();
                entry.size = binaryReader.ReadInt32();
                entry.compSize = binaryReader.ReadInt32();
                entry.zipped = binaryReader.ReadBoolean();
                entry.checkSum = binaryReader.ReadInt32();
                entry.path_length = binaryReader.ReadInt32();
                entry.name = new String(binaryReader.ReadChars(entry.path_length));
                entry.buffer = new Byte[entry.size];
                entries.Add(entry);
            }
            binaryReader.Close();
        }

        public WizardReader(String filename)
        {
            binaryReader = new BinaryReader(File.Open(filename, FileMode.Open));
            entries = new List<WadEntry>();
            ReadHeader();
            GetEntries();
            GetEntryBuffers(filename);
        }

        public void Extract(String output)
        {
            
            DumpEntries(output);
        }

        public long GetLength()
        {
            
            return binaryReader.BaseStream.Length;
        }

        public int GetFileCount()
        {
            return header.entryCount;
        }

        
        public List<WadEntry> GetWadData()
        {
            return entries;
        }
    }
}
