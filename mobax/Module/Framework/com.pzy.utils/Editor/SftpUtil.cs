using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Renci.SshNet;
using System.IO;
using System.Threading.Tasks;

public class SftpUtil : MonoBehaviour
{
    public static void UploadAsync(string host, int port, string username, string password, string localFilePath, string remoteFilePath)
    {
        //string host = "sftp.example.com";
        //int port = 22;
        //string username = "username";
        //string password = "password";
        //string localFilePath = @"C:\path\to\local\file.txt";
        //string remoteFilePath = "/remote/path/file.txt";

        // 创建一个SSH连接
        using (var client = new SftpClient(host, port, username, password))
        {
            // 连接到远程服务器
            client.Connect();

            // 检查远程目录是否存在，如果不存在则创建
            if (!client.Exists(Path.GetDirectoryName(remoteFilePath)))
            {
                client.CreateDirectory(Path.GetDirectoryName(remoteFilePath));
            }

            // 打开本地文件并将其上传到远程服务器
            using (var fileStream = new FileStream(localFilePath, FileMode.Open))
            {
                Debug.Log($"Upload... {fileStream.Length}k");
                client.UploadFile(fileStream, remoteFilePath);
                Debug.Log($"Upload complete.");
            }

            // 断开与远程服务器的连接
            client.Disconnect();
        }
    }
}
