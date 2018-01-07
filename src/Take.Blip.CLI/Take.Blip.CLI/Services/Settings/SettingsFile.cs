using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Lime.Protocol;

namespace Take.BlipCLI.Services.Settings
{
    public class SettingsFile : ISettingsFile
    {
        private const string SETTINGS_FILE_PATH = @".\settings.blip";
        private readonly List<NodeCredential> _nodeCredentialsList;
        private readonly AESEncryptor _aESEncryptor;

        public SettingsFile()
        {
            _aESEncryptor = new AESEncryptor();
            _nodeCredentialsList = GetAllNodeCredentials();
        }

        public void AddNodeCredentials(NodeCredential nodeCredential)
        {
            File.AppendAllText(SETTINGS_FILE_PATH, EncryptNodeCredentials(nodeCredential));
        }

        public NodeCredential GetNodeCredentials(Node node)
        {
            return _nodeCredentialsList.FirstOrDefault(n => n.Node.Equals(node));
        }

        private string EncryptNodeCredentials(NodeCredential nodeCredential)
        {
            var row = $"{nodeCredential.Node.ToIdentity().ToString()}:{nodeCredential.Authorization}";
            return _aESEncryptor.Encrypt(row);
        }

        private List<NodeCredential> GetAllNodeCredentials()
        {
            var credentialsList = new List<NodeCredential>();
            // Create a settings file if not exists.
            if (!File.Exists(SETTINGS_FILE_PATH)) File.CreateText(SETTINGS_FILE_PATH);

            // Open the file to read from.
            using (StreamReader sr = File.OpenText(SETTINGS_FILE_PATH))
            {
                string row;
                while ((row = sr.ReadLine()) != null)
                {
                    var decryptedRow = _aESEncryptor.Decrypt(row);
                    credentialsList.Add(new NodeCredential
                    {
                        Node = decryptedRow.Split(':')[0],
                        Authorization = decryptedRow.Split(':')[1],
                    });
                }
            }

            return credentialsList;
        }
    }

    public interface ISettingsFile
    {
        void AddNodeCredentials(NodeCredential nodeCredential);
        NodeCredential GetNodeCredentials(Node node);
    }
}
