using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace WFDProcess
{
    public class LoadFileForChart
    {
        static ProgressBar _progress = new ProgressBar();
        public string FileName { get; set; }
        public int LoadBufferSize { get; set; }
        public long FileSize { get; set; }
        public float ReSamplePercentage { get; set; }
        public int ReSampleRate { get; set; }
        public int ExamplePercent { get; set; }
        public long ExampleSize { get; set; }

        const int _reSamplePercentageLimit = 100;
        const int _exampleDefaultSize = 100;
        const int _loadBufferSizeLimit = 253 * 1024 * 1024;
        const int _byteBufferSizeLimitation = 2130000000;
        const int _doubleBufferSizeLimitation = 266000000;
        const int _intBufferSizeLimitation = 532000000;
        const int _shortBufferSizeLimitation = 1065000000;
        public LoadFileForChart(string filePath,float reSamplePercentage,int examplePercent)
        {
            try
            {
                if(reSamplePercentage <= _reSamplePercentageLimit && examplePercent< _exampleDefaultSize)
                {
                FileInfo fileInfo = new FileInfo(filePath);
                this.FileName = fileInfo.FullName;
                this.FileSize = fileInfo.Length;
                this.ReSamplePercentage = reSamplePercentage;
                this.ExamplePercent = examplePercent;
                this.ExampleSize = FileSize * ExamplePercent / 100;
                this.LoadBufferSize = 1024 * 1024;
                this.ReSampleRate = (int)(100/reSamplePercentage);
                }
                else if(reSamplePercentage> _reSamplePercentageLimit)
                {
                    MessageBox.Show("lotfan addad manteghi baraye resample vared konid");
                }
                else
                {
                    MessageBox.Show("100% ke dige example nist");
                }
            }
            catch
            {
                MessageBox.Show("couldnt get file data!");
            }
            
        }
        public LoadFileForChart(string filePath, float reSamplePercentage, int examplePercent,int loadBufferSize)
        {
            try
            {
                if (reSamplePercentage <= _reSamplePercentageLimit && examplePercent < _exampleDefaultSize && loadBufferSize<= _loadBufferSizeLimit)
                {
                FileInfo fileInfo = new FileInfo(filePath);
                this.FileName = fileInfo.FullName;
                this.FileSize = fileInfo.Length;
                this.ReSamplePercentage = reSamplePercentage;
                this.ExamplePercent = examplePercent;
                this.ExampleSize = FileSize * ExamplePercent / 100;
                this.LoadBufferSize = loadBufferSize;
                this.ReSampleRate = (int)(100 / reSamplePercentage);
            
                }
                else if (reSamplePercentage > _reSamplePercentageLimit)
                {
                    MessageBox.Show("lotfan addad manteghi baraye resample vared konid");
                }
                else if (examplePercent > _exampleDefaultSize)
                {
                    MessageBox.Show("100% ke dige example nist");
                }
                else
                {
                    MessageBox.Show("buffer size is limited at 253MB");
                }
            }
            catch
            {
                MessageBox.Show("couldnt get file data!");
            }
        }

        public byte[] ProcessDataByte(String filePath)
        {
            if ((FileSize / ReSampleRate) + 1 > _byteBufferSizeLimitation)
            {
                long ResampleMin = FileSize / (_byteBufferSizeLimitation - 1);
                MessageBox.Show("cant process with double using current ResampleRate,your ResampleRate should be at least {0}", ResampleMin.ToString());
                return new byte[1];
            }
            int flag = 0;
            BinaryReader br;
            using (br = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            {
                MessageBox.Show("loading file data!");
                byte[] fileContent = null;
                byte[] convertedData = new byte[(FileSize /ReSampleRate)+1];
                long progress = 0;
                for (int count = 0; count < (FileSize / LoadBufferSize); count++)
                {
                    int j = 0;
                    fileContent = br.ReadBytes(LoadBufferSize);
                    for (int k = 0; k < fileContent.Length; k += ReSampleRate)
                    {
                        try
                        {
                            convertedData[(count * fileContent.Length / ReSampleRate) + j] = fileContent[k];
                            j++;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            MessageBox.Show("could not load the file completely");
                            goto lable;
                        }
                        catch
                        {
                            MessageBox.Show("something went wrong");
                        }
                    }
                    progress = (count * fileContent.Length) / (FileSize / 100);
                    //ConsoleUtility.WriteProgressBar((int)progress, true);
                    //if (progress == 99)
                    //{
                    //    //ConsoleUtility.WriteProgressBar(100, true);
                    //}
                    if (br.BaseStream.Position >= ExampleSize + LoadBufferSize && flag == 0)    //(count * fileContent.Length / ReSampleRate) + j -> baraye tabdil nesbat b examplesize/filesize
                    {
                        flag = 1;
                        //ConsoleUtility.WriteProgressBar(ExamplePercent, true);
                        MessageBox.Show("Loading Example done!");
                    }
                }
            lable:
                {
                    MessageBox.Show("file loaded completely!");
                    return convertedData;
                }
            }
        }


        public double[] ProcessDataDouble(String filePath)
        {
            int loadflag = 0;
            int flag = 0;
            BinaryReader br;
            using (br = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            {
                if ((FileSize / ReSampleRate) + 1 > _doubleBufferSizeLimitation)
                {
                    long ResampleMin = FileSize / (_doubleBufferSizeLimitation -1) ;
                    MessageBox.Show("cant process with double using current ResampleRate,your ResampleRate should be at least {0}",ResampleMin.ToString());
                    return new double[1];
                }
                MessageBox.Show("loading file data!");
                //ConsoleUtility.WriteProgressBar(0);
                double[] fileContent = new double[LoadBufferSize ];
                double[] convertedData = new double[(FileSize / ReSampleRate) + 1];
                //double[] convertedData = (FileSize / ReSampleRate) + 1 > 266000000 ? new double[266000000] : new double[(FileSize / ReSampleRate) + 1];
                int contentSize = fileContent.Length;
                long progress = 0;
                for (int count = 0; count < (FileSize / LoadBufferSize); count++)
                {
                    int j = 0;
                    for (int i = 0; i < fileContent.Length; i++)
                    {
                        try
                        {
                            fileContent[i] = br.ReadDouble();
                        }
                        catch (EndOfStreamException)
                        {
                            //MessageBox.Show(br.BaseStream.Position);
                            //MessageBox.Show(FileSize);
                            loadflag = 1;
                            contentSize = i;
                            break;
                        }
                    }



                    for (int k = 0; k < contentSize; k += ReSampleRate)
                    {
                        try
                        {
                            convertedData[(count * fileContent.Length / ReSampleRate) + j] = fileContent[k];
                            j++;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            MessageBox.Show("could not load the file completely");
                            goto lable;
                        }
                        catch
                        {
                            MessageBox.Show("something went wrong");
                        }
                    }

                    progress = ((count + 1) * fileContent.Length * 8) / (FileSize / 100);
                   // ConsoleUtility.WriteProgressBar((int)progress, true);
                    //if (progress == 99)
                    //{
                    //    ConsoleUtility.WriteProgressBar(100, true);
                    //}
                    if (br.BaseStream.Position >= ExampleSize && flag == 0)
                    {
                        flag = 1;
                        if (progress == ExamplePercent)
                        {
                            //ConsoleUtility.WriteProgressBar(ExamplePercent, true);
                        }
                        MessageBox.Show("Loading Example done!");
                    }
                    // MessageBox.Show(count);
                    //if (count == 1023)
                    //    MessageBox.Show(br.BaseStream.Position);
                    if (loadflag == 1)
                    {
                        //MessageBox.Show(count);
                        break;
                    }

                }
            lable:
                {
                    MessageBox.Show("file loaded completely!");
                    return convertedData;
                }
            }
            
        }

    
        public Int32[] ProcessDataInt32(String filePath)
        {
            if ((FileSize / ReSampleRate) + 1 > _intBufferSizeLimitation)
            {
                long ResampleMin = FileSize / (_intBufferSizeLimitation - 1);
                MessageBox.Show("cant process with double using current ResampleRate,your ResampleRate should be at least {0}", ResampleMin.ToString());
                
                return new Int32[1];
            }
            int loadflag = 0;
            int flag = 0;
            BinaryReader br;
            using (br = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            {
                MessageBox.Show("loading file data!");
                //ConsoleUtility.WriteProgressBar(0);
                Int32[] fileContent = new Int32[LoadBufferSize];
                Int32[] convertedData = new Int32[(FileSize / ReSampleRate) + 1];
                int contentSize = fileContent.Length;
                long progress = 0;
                for (int count = 0; count < (FileSize / LoadBufferSize); count++)
                {
                    int j = 0;
                    for (int i = 0; i < fileContent.Length; i++)
                    {
                        try
                        {
                            fileContent[i] = br.ReadInt32();
                        }
                        catch (EndOfStreamException)
                        {
                            //MessageBox.Show(br.BaseStream.Position);
                            //MessageBox.Show(FileSize);
                            loadflag = 1;
                            contentSize = i;
                            break;
                        }
                    }



                    for (int k = 0; k < contentSize; k += ReSampleRate)
                    {
                        try
                        {
                            convertedData[(count * fileContent.Length / ReSampleRate) + j] = fileContent[k];
                            j++;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            MessageBox.Show("could not load the file completely");
                            goto lable;
                        }
                        catch
                        {
                            MessageBox.Show("something went wrong");
                        }
                    }

                    progress = ((count + 1) * fileContent.Length * 4) / (FileSize / 100);
                    //ConsoleUtility.WriteProgressBar((int)progress, true);
                    //if (progress == 99)
                    //{
                    //    ConsoleUtility.WriteProgressBar(100, true);
                    //}
                    if (br.BaseStream.Position >= ExampleSize && flag == 0)
                    {
                        flag = 1;
                        if (progress == ExamplePercent)
                        {
                            //ConsoleUtility.WriteProgressBar(ExamplePercent, true);
                        }
                        MessageBox.Show("Loading Example done!");
                    }
                    // MessageBox.Show(count);
                    //if (count == 1023)
                    //    MessageBox.Show(br.BaseStream.Position);
                    if (loadflag == 1)
                    {
                        //MessageBox.Show(count);
                        break;
                    }

                }
            lable:
                {
                    MessageBox.Show("file loaded completely!");
                    return convertedData;
                }
            }
        } 


        public Int16[] ProcessDataInt16(String filePath)
        {
            if ((FileSize / ReSampleRate) + 1 > _shortBufferSizeLimitation)
            {
                long ResampleMin = FileSize / (_shortBufferSizeLimitation - 1);
                MessageBox.Show("cant process with double using current ResampleRate,your ResampleRate should be at least {0}", ResampleMin.ToString());
                
                return new Int16[1];
            }
            int loadflag = 0;
            int flag = 0;
            BinaryReader br;
            using (br = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            {
                MessageBox.Show("loading file data!");
                //ConsoleUtility.WriteProgressBar(0);
                Int16[] fileContent = new Int16[LoadBufferSize];
                Int16[] convertedData = new Int16[(FileSize / ReSampleRate) + 1];
                int contentSize = fileContent.Length;
                long progress = 0;
                for (int count = 0; count < (FileSize / LoadBufferSize); count++)
                {
                    int j = 0;
                    for (int i = 0; i < fileContent.Length; i++)
                    {
                        try
                        {
                            fileContent[i] = br.ReadInt16();
                        }
                        catch (EndOfStreamException)
                        {
                            //MessageBox.Show(br.BaseStream.Position);
                            //MessageBox.Show(FileSize);
                            loadflag = 1;
                            contentSize = i;
                            break;
                        }
                    }



                    for (int k = 0; k < contentSize; k += ReSampleRate)
                    {
                        try
                        {
                            convertedData[(count * fileContent.Length / ReSampleRate) + j] = fileContent[k];
                            j++;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            MessageBox.Show("could not load the file completely");
                            goto lable;
                        }
                        catch
                        {
                            MessageBox.Show("something went wrong");
                        }
                    }

                    progress = ((count + 1) * fileContent.Length * 2) / (FileSize / 100);
                    //ConsoleUtility.WriteProgressBar((int)progress, true);
                    //if (progress == 99)
                    //{
                    //    ConsoleUtility.WriteProgressBar(100, true);
                    //}
                    if (br.BaseStream.Position >= ExampleSize && flag == 0)
                    {
                        flag = 1;
                        if (progress == ExamplePercent)
                        {
                            //ConsoleUtility.WriteProgressBar(ExamplePercent, true);
                        }
                        MessageBox.Show("Loading Example done!");
                    }
                    // MessageBox.Show(count);
                    //if (count == 1023)
                    //{
                    //    MessageBox.Show(br.BaseStream.Position);
                    //    MessageBox.Show(count);
                    //}
                    if (loadflag == 1)
                    {
                        //MessageBox.Show(count);
                        break;
                    }

                }
            lable:
                {
                    MessageBox.Show("file loaded completely!");
                    return convertedData;
                }
            }

        }

    }
}
