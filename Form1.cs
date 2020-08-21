using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.MediaFoundation;
using NAudio.Wave;
using NAudio.Codecs;
using NAudio.Utils;
using System.Threading;
using NAudio.FileFormats;
using NVorbis.Ogg;
using NVorbis;
using Pfim.dds;
using NAudio.Vorbis;
using System.Drawing;
using System.Runtime.InteropServices;

using System.Drawing.Imaging;

namespace Wiz101Extractor
{
    
    public partial class Form1 : Form
    {
        
        private WaveOutEvent outputDevice;
        WizardReader reader;
        //todo: file select
        String fileName = "";
        String output = "output/";
        OpenFileDialog dialog;
        FolderBrowserDialog folderBrowser;
        public Form1()
        {
            InitializeComponent();
            dialog = new OpenFileDialog();
            folderBrowser = new FolderBrowserDialog();

            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
            }
            listBox1.Hide();
            button2.Hide();
            button3.Hide();
            label2.Hide();
            label3.Hide();
            checkBox1.Hide();
            ddspreview.Hide();
        }   

        
     
        //select WAD file
        private void button1_Click(object sender, EventArgs e)
        {

            dialog.ShowDialog();
            fileName = dialog.FileName;

            reader = new WizardReader(fileName);

            listBox1.Items.Clear();
            listBox1.Show();
            foreach (WadEntry entry in reader.GetWadData())
            {
                listBox1.Items.Add(entry.name);
            }
            label4.Text = "Version: " + reader.GetHeader().version.ToString();
            label5.Text = "Entry Count: " + reader.GetHeader().entryCount.ToString();

            button2.Show();


        }

        //select output folder
        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowser.ShowDialog();
            output = folderBrowser.SelectedPath + "/Wizard101Extracted/";
            button3.Show();
        }

        private void ExtractFiles()
        {
            System.IO.Directory.CreateDirectory(output);



            reader.Extract(output);
        }

        //extract (That's the go button!"
        private void button3_Click(object sender, EventArgs e)
        {
            
            ThreadStart childref = new ThreadStart(ExtractFiles);
            Thread childThread = new Thread(childref);
            childThread.Start();
            

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void PlayWav(MemoryStream stream)
        {
            WaveFileReader wav = new WaveFileReader(stream);
            outputDevice.Dispose();
            outputDevice.Init(wav);
            outputDevice.Play();
        }
        private void PlayMP3(MemoryStream stream)
        {
            outputDevice.Dispose();
            Mp3FileReader mp3 = new Mp3FileReader(stream);
            outputDevice.Init(mp3);
            outputDevice.Play();
        }

        private void PlayOgg(MemoryStream stream)
        {
            outputDevice.Dispose();
            VorbisWaveReader ogg = new VorbisWaveReader(stream);
            outputDevice.Init(ogg);
            outputDevice.Play();

        }

        private void DisplayDDS(MemoryStream stream)
        {
            PixelFormat format;

            ddspreview.Show();
            var image = Pfim.Pfim.FromStream(stream);
            switch (image.Format)
            {
                case Pfim.ImageFormat.Rgb24:
                    format = PixelFormat.Format24bppRgb;
                    break;

                case Pfim.ImageFormat.Rgba32:
                    format = PixelFormat.Format32bppArgb;
                    break;

                case Pfim.ImageFormat.R5g5b5:
                    format = PixelFormat.Format16bppRgb555;
                    break;

                case Pfim.ImageFormat.R5g6b5:
                    format = PixelFormat.Format16bppRgb565;
                    break;

                case Pfim.ImageFormat.R5g5b5a1:
                    format = PixelFormat.Format16bppArgb1555;
                    break;

                case Pfim.ImageFormat.Rgb8:
                    format = PixelFormat.Format8bppIndexed;
                    break;

                default:
                    var msg = $"{image.Format} is not recognized for Bitmap on Windows Forms. " +
                               "You'd need to write a conversion function to convert the data to known format";
                    var caption = "Unrecognized format";
                    MessageBox.Show(msg, caption, MessageBoxButtons.OK);
                    return;
            }
            var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
            var bitmap = new Bitmap(image.Width, image.Height, image.Stride, format, ptr);
            ddspreview.Image = bitmap;
            ddspreview.Width = bitmap.Width;
            ddspreview.Height = bitmap.Height;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label2.Show();
            label3.Show();
            checkBox1.Show();

            WadEntry entry = reader.GetWadData()[listBox1.SelectedIndex];
            checkBox1.Checked = entry.zipped;
            label2.Text = "Uncompressed size: " + entry.size.ToString();
            label3.Text = "Compressed size: " + entry.compSize.ToString();
            MemoryStream stream = new MemoryStream(entry.buffer);
            ddspreview.Hide();
            if (entry.name.Contains("wav"))
            {
                PlayWav(stream);
            }else if (entry.name.Contains("mp3"))
            {
                PlayMP3(stream);
            }else if (entry.name.Contains("dds"))
            {
                DisplayDDS(stream);
                ddspreview.Show();
            }else if (entry.name.Contains("ogg"))
            {
                PlayOgg(stream);
            }
          
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
