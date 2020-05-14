using UnityEngine;
    using System.Collections;
    using System;
    using System.Net;
    using System.IO;
    
    public class Uploader : MonoBehaviour
    {
        public static readonly string FTPHost = "ftp://cloud.digitopolisstudio.com//MrEss";
        public static readonly string FTPUserName = "digitopolis";
        public static readonly string FTPPassword = "tuangmon";
        public static string FilePath;
    
        public static void UploadFile(string name)
        {
            FilePath = Path.Combine(Application.persistentDataPath, name);
            Debug.Log("Path: " + FilePath);
            
            WebClient client = new WebClient();
            Uri uri = new Uri(FTPHost + new FileInfo(FilePath).Name);
    
            client.UploadProgressChanged += OnFileUploadProgressChanged;
            client.UploadFileCompleted += OnFileUploadCompleted;
            client.Credentials = new NetworkCredential(FTPUserName, FTPPassword);
            client.UploadFileAsync(uri, "STOR", FilePath);
        }
        static void OnFileUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            Debug.Log("Uploading Progreess: " + e.ProgressPercentage);
        }

        static void OnFileUploadCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            Debug.Log("File Uploaded");
        }
   
    }
