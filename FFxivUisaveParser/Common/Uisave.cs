using Prism.Events;
using System;
using System.IO;
using System.Xml;

namespace FFxivUisaveParser.Common
{
    internal class Uisave
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IEventAggregator eventAggregator;

        public Uisave(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.GetEvent<FilePathEvent>().Subscribe(arg =>
            {
                Parse(arg);
            });
        }

        private bool Parse(string fileName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            var rootEle = xmlDoc.CreateElement("root");
            xmlDoc.AppendChild(rootEle);

            if (!BitConverter.IsLittleEndian)
            {
                logger.Error("BitConverter is reporting Big-Endian");
                return false;
            }

            if (!File.Exists(fileName))
            {
                logger.Error($"cannot open file {fileName}");
                return false;
            }
            
            byte[] rawData = File.ReadAllBytes(fileName);
            int rawDataOffset = 0;
            
            // parse header
            if (rawDataOffset + 16 > rawData.Length)
            {
                logger.Error("insufficient data to read raw data header");
                return false;
            }
            int decryptedLength = BitConverter.ToInt32(rawData, rawDataOffset + 8);
            rawDataOffset += 16;

            // decrypt data
            if (rawDataOffset + decryptedLength > rawData.Length)
            {
                logger.Error("insufficient data to read encrypted data");
                return false;
            }
            byte[] decryptedData = XorDecrypt(rawData, rawDataOffset, decryptedLength, 0x31);
            int decryptedDataOffset = 0;

            // parse content id
            if (decryptedDataOffset + 16 > decryptedData.Length)
            {
                logger.Error("insufficient data to read decrypted data header");
                return false;
            }
            ulong contentId = BitConverter.ToUInt64(decryptedData, decryptedDataOffset + 8);
            decryptedDataOffset += 16;
            var contentIdEle = xmlDoc.CreateElement("content_id");
            contentIdEle.InnerText = contentId.ToString("X16");
            rootEle.AppendChild(contentIdEle);

            while (decryptedDataOffset + 16 <= decryptedData.Length)
            {
                XmlElement sectionEle = xmlDoc.CreateElement("section");

                // parse section header
                ushort sectionIndex = BitConverter.ToUInt16(decryptedData, decryptedDataOffset);
                decryptedDataOffset += 8;
                int sectionDataLength = BitConverter.ToInt32(decryptedData, decryptedDataOffset);
                decryptedDataOffset += 8;
                var idAttr = xmlDoc.CreateAttribute("id");
                idAttr.Value = sectionIndex.ToString();
                sectionEle.Attributes.Append(idAttr);

                var sectionDataLengthEle = xmlDoc.CreateElement("length");
                sectionDataLengthEle.InnerText = sectionDataLength.ToString();
                sectionEle.AppendChild(sectionDataLengthEle);

                // parce section data
                if (decryptedDataOffset + sectionDataLength > decryptedData.Length)
                {
                    logger.Error("insufficient data to read section data");
                    return false;
                }

                switch (sectionIndex)
                {
                    case 0x11:
                        ParseWaymarkPresets(decryptedData, decryptedDataOffset, sectionDataLength, ref xmlDoc, ref sectionEle);
                        break;
                    default:
                        break;
                }
                decryptedDataOffset += sectionDataLength;

                // Parse trailing data
                if (decryptedDataOffset + 4 > decryptedData.Length)
                {
                    logger.Error("insufficient data to read trailing data");
                    return false;
                }
                decryptedDataOffset += 4;

                rootEle.AppendChild(sectionEle);
            }

            eventAggregator.GetEvent<xmlEvent>().Publish(xmlDoc);
            return true;
        }

        private byte[] XorDecrypt(byte[] rawData, int startIndex, int length, byte xorValue)
        {
            byte[] decryptedData = new byte[length];
            for (int i = 0; i < length; ++i)
            {
                decryptedData[i] = (byte)(rawData[startIndex + i] ^ xorValue);
            }
            return decryptedData;
        }

        private const int kPresetNum = 5;
        private const int kWaymarkNum = 8;
        private static readonly string[] kWaymarkName = { "A", "B", "C", "D", "1", "2", "3", "4" };

        private bool ParseWaymarkPresets(byte[] decryptedData, int startIndex, int length,
            ref XmlDocument xmlDoc, ref XmlElement sectionEle)
        {
            uint x_ori, y_ori, z_ori;
            double x_real, y_real, z_real;
            byte enableFlag;
            ushort zoneId;
            uint timestamp;
            int offset = 16;    // skip header
            for (int i = 0; i < kPresetNum; ++i)
            {
                if (offset + 104 > length)
                {
                    logger.Error($"insufficient data to read {i}th preset data");
                    return false;
                }

                var presetEle = xmlDoc.CreateElement($"preset_{i}");
                for (int j = 0; j < kWaymarkNum; ++j)
                {
                    x_ori = BitConverter.ToUInt32(decryptedData, startIndex + offset);
                    y_ori = BitConverter.ToUInt32(decryptedData, startIndex + offset + 4);
                    z_ori = BitConverter.ToUInt32(decryptedData, startIndex + offset + 8);
                    x_real = (int)x_ori / 1000.0;
                    y_real = (int)y_ori / 1000.0;
                    z_real = (int)z_ori / 1000.0;
                    offset += 12;

                    var waymarkEle = xmlDoc.CreateElement($"waymark_{kWaymarkName[j]}");
                    waymarkEle.InnerText = String.Format("({0:X8}, {1:X8}, {2:X8}) == ({3}, {4}, {5})",
                        x_ori, y_ori, z_ori, x_real, y_real, z_real);
                    presetEle.AppendChild(waymarkEle);
                }

                enableFlag = decryptedData[startIndex + offset];
                zoneId = BitConverter.ToUInt16(decryptedData, startIndex + offset + 2);
                timestamp = BitConverter.ToUInt32(decryptedData, startIndex + offset + 4);
                offset += 8;

                var enableFlagEle = xmlDoc.CreateElement("enable_flag");
                enableFlagEle.InnerText = Convert.ToString(enableFlag, 2);
                presetEle.AppendChild(enableFlagEle);
                var zoneIdEle = xmlDoc.CreateElement("zone_id");
                zoneIdEle.InnerText = $"{zoneId}";
                presetEle.AppendChild(zoneIdEle);
                var timestampEle = xmlDoc.CreateElement("timestamp");
                timestampEle.InnerText = $"{timestamp}";
                presetEle.AppendChild(timestampEle);

                sectionEle.AppendChild(presetEle);
            }

            return true;
        }
    }
}
