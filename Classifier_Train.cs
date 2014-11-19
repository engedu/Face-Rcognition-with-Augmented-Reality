﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.CV.UI;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Drawing.Imaging;
using System.Drawing;

/// <summary>
/// Desingned to remove the training a EigenObjectRecognizer code from the main form
/// </summary>
namespace EMGUCV
{
    public class Classifier_Train : IDisposable
    {

        #region Variables

        //Eigen
        MCvTermCriteria termCrit;
        EigenObjectRecognizer recognizer;

        //training variables
        Image<Gray, byte>[] trainingImages;//Images
        List<string> allname = new List<string>(); //labels
        
        float Eigen_Distance = 0;
        string Eigen_label;
        int Eigen_threshold = 0;

        //Class Variables
        string Error;
        bool _IsTrained = false;
        DBConn mydb ;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor, Looks in (Application.StartupPath + "\\TrainedFaces") for traing data.
        /// </summary>
        public Classifier_Train()
        {
            
            _IsTrained = LoadTrainingData();
        }

        
        #endregion

        public bool IsTrained
        {
            get { return _IsTrained; }
        }
        /*public Image<Gray, float> getAVGImage()
        {
            Image<Gray, float> blankImage = new Image<Gray, float>(200, 200);
            try
            {
                if (_IsTrained)
                {
                    Image<Gray, float> retImage = recognizer.AverageImage;

                    return retImage;
                }
                else return blankImage;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return blankImage;
            }
        }*/
        public Image<Gray, byte>[] getTrainingImage()
        {
            if (_IsTrained)
            {
                return trainingImages;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Recognise a Grayscale Image using the trained Eigen Recogniser
        /// </summary>
        /// <param name="Input_image"></param>
        /// <returns></returns>
        public void reloadData()
        {
            _IsTrained = LoadTrainingData();
        }
        public Image<Gray,float>[] getEigenfaceArray()
        {
            if(_IsTrained){
                return recognizer.EigenImages;
            }
            else
            {
                return null;
            }
        }
        public string Recognise(Image<Gray, byte> Input_image, int Eigen_Thresh = -1)
        {
            try
            {
                
                if (_IsTrained)
                {
                    recognizer.EigenDistanceThreshold =10000;
                    EigenObjectRecognizer.RecognitionResult ER = recognizer.Recognize(Input_image);
                    
                    if (ER == null)
                    {
                        Eigen_label = "UnknownNull";
                        Eigen_Distance = 0;
                        return Eigen_label;
                    }
                    else
                    {
                        Eigen_label = ER.Label;
                        Eigen_Distance = ER.Distance;

                        if (Eigen_Thresh > -1)
                        {
                            Eigen_threshold = Eigen_Thresh;
                        }
                        if (Eigen_Distance > Eigen_threshold)
                        {
                            return Eigen_label;
                        }
                            // + " " + Eigen_Distance.ToString()
                        else
                        {
                            return "UnknownFace";
                        }
                    }

                }
                else return "";
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return "";
            }
            
        }

        public int Set_Eigen_Threshold
        {
            set
            {
                 recognizer.EigenDistanceThreshold = value;
            }
        }

        public string Get_Eigen_Label
        {
            get
            {
                return Eigen_label;
            }
        }

        public float Get_Eigen_Distance
        {
            get
            {
                //get eigenDistance
                return Eigen_Distance;
            }
        }

        public string Get_Error
        {
            get { return Error; }
        }

        public void Dispose()
        {
            recognizer = null;
            trainingImages = null;
            allname = null;
            Error = null;
            GC.Collect();
        }
        
        Bitmap NormalizeLbpMatrix(double[,] Mat, Bitmap lbp, double max)
        {
            int NumRow = lbp.Height;
            int numCol = lbp.Width;
            for (int i = 0; i < NumRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {
                    // see the Normalization process of dividing pixel by max value and multiplying with 255
                    double d = Mat[j, i] / max;
                    int v = (int)(d * 255);
                    Color c = Color.FromArgb(v, v, v);
                    lbp.SetPixel(j, i, c);
                }
            }
            return lbp;
        }
        double Bin2Dec(List<int> bin)
        {
            double d = 0;

            for (int i = 0; i < bin.Count; i++)
            {
                d += bin[i] * Math.Pow(2, i);
            }
            return d;
        }
        private bool LoadTrainingData()
        {
            mydb = new DBConn();
            allname = mydb.getLabelList();
            trainingImages = mydb.getTrainedImageList();
                
                if (mydb.getImageCount() > 0)
                {
                    
                    if (trainingImages.Length != 0)
                    {
                        //set round and ...
                        termCrit = new MCvTermCriteria(mydb.getImageCount(), 0.001);
                         //Eigen face recognizer
                        recognizer = new EigenObjectRecognizer(trainingImages,allname.ToArray(),11000,ref termCrit);
                        return true;
                    }
                    else
                    {
                        return false;
                    }                    
                }
                else
                {
                    return false;
                }           
        }
    }
}
