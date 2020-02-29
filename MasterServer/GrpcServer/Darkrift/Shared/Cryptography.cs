/*
    Credits for the class to LestaAllmaron : https://github.com/lestaallmaron
*/
using DarkRift;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;



namespace MasterServer.DarkRift.Shared
{

    public static class Cryptography
    {


        public static byte[] GenerateAESKey()
        {
            using (Aes aesProvider = Aes.Create())
            {
                aesProvider.GenerateKey();
                return aesProvider.Key;
            }
        }

        public static void GenerateRSAKeys(out RSAParameters privateKey, out RSAParameters publicKey)
        {
            var csp = new RSACryptoServiceProvider(2048);

            privateKey = csp.ExportParameters(true);
            publicKey = csp.ExportParameters(false);
        }

        public static string RsaKeyToString(RSAParameters key)
        {
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, key);
            return sw.ToString();
        }


        public static RSAParameters StringToRsaKey(string value)
        {
            StringReader sr = new StringReader(value);
            XmlSerializer xs = new XmlSerializer(typeof(RSAParameters));
            return (RSAParameters)xs.Deserialize(sr);
        }

        public static DarkRiftWriter EncryptWriterAES(DarkRiftWriter writer, byte[] key)
        {
            Message message = Message.Create(0, writer);
            DarkRiftReader reader = message.GetReader();
            byte[] data = reader.ReadRaw(reader.Length);
            data = Encrypt_Aes(data, key);
            writer = DarkRiftWriter.Create();
            writer.WriteRaw(data, 0, data.Length);

            message.Dispose();
            reader.Dispose();
            return writer;
        }

        public static DarkRiftReader DecryptReaderAES(DarkRiftReader reader, byte[] key)
        {
            byte[] data = reader.ReadRaw(reader.Length);

            data = Decrypt_Aes(data, key);

            DarkRiftWriter writer = DarkRiftWriter.Create();
            writer.WriteRaw(data, 0, data.Length);
            Message message = Message.Create(0, writer);
            DarkRiftReader newReader = message.GetReader();
            message.Dispose();
            writer.Dispose();
            reader.Dispose();
            return newReader;
        }


        public static DarkRiftReader DecryptReaderRSA(this DarkRiftReader reader, RSAParameters privateKey)
        {
            if (reader.Length > 256)
            {
                throw new ArgumentOutOfRangeException(nameof(reader), "reader mustn't contain more then 256 bytes");
            }

            byte[] data = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                data[i] = reader.ReadByte();
            }

            data = DecryptRSA(data, privateKey);

            DarkRiftWriter writer = DarkRiftWriter.Create();
            writer.WriteRaw(data, 0, data.Length);
            Message message = Message.Create(0, writer);
            DarkRiftReader newReader = message.GetReader();
            message.Dispose();
            writer.Dispose();
            reader.Dispose();
            return newReader;
        }

        public static DarkRiftWriter EncryptWriterRSA(this DarkRiftWriter writer, RSAParameters publicKey)
        {

            Message message = Message.Create(0, writer);
            DarkRiftReader reader = message.GetReader();
            byte[] data = reader.ReadRaw(reader.Length);
            data = EncryptRSA(data, publicKey);
            writer = DarkRiftWriter.Create();
            writer.WriteRaw(data, 0, data.Length);

            message.Dispose();
            reader.Dispose();
            return writer;
        }


        private static byte[] DecryptRSA(byte[] message, RSAParameters privateKey)
        {
            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(2048);
            csp.ImportParameters(privateKey);


            return csp.Decrypt(message, false);
        }

        private static byte[] EncryptRSA(byte[] message, RSAParameters publicKey)
        {
            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(2048);
            csp.ImportParameters(publicKey);
            byte[] encrypted = csp.Encrypt(message, false);
            return encrypted;
        }

        private static byte[] Encrypt_Aes(byte[] plain, byte[] key)
        {
            byte[] encrypted;
            byte[] IV;
            using (MemoryStream mstream = new MemoryStream())
            {
                using (Aes aesProvider = Aes.Create())
                {
                    aesProvider.GenerateIV();
                    IV = aesProvider.IV;

                    using (CryptoStream cryptoStream = new CryptoStream(mstream, aesProvider.CreateEncryptor(key, IV), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plain, 0, plain.Length);
                    }
                    encrypted = mstream.ToArray();
                }
            }

            var combined = new byte[IV.Length + encrypted.Length + 4];
            Array.Copy(IV, 0, combined, 0, IV.Length);
            Array.Copy(encrypted, 0, combined, IV.Length, encrypted.Length);
            Array.Copy(BitConverter.GetBytes(plain.Length), 0, combined, IV.Length + encrypted.Length, 4);
            return combined;
        }


        private static byte[] Decrypt_Aes(byte[] encrypted, byte[] key)
        {
            byte[] plain;
            byte[] IV;
            byte[] size = new byte[4];
            using (Aes aesProvider = Aes.Create())
            {
                aesProvider.Key = key;
                IV = new byte[aesProvider.BlockSize / 8];
                byte[] cipherText = new byte[encrypted.Length - IV.Length - 4];

                Array.Copy(encrypted, IV, IV.Length);
                Array.Copy(encrypted, IV.Length, cipherText, 0, cipherText.Length);
                Array.Copy(encrypted, IV.Length + cipherText.Length, size, 0, 4);

                using (MemoryStream mStream = new MemoryStream(cipherText))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(mStream, aesProvider.CreateDecryptor(key, IV), CryptoStreamMode.Read))
                    {
                        plain = new byte[BitConverter.ToInt32(size, 0)];
                        cryptoStream.Read(plain, 0, BitConverter.ToInt32(size, 0));
                    }
                }
            }

            return plain;
        }

    }
}
