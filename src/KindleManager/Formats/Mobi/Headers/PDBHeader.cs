﻿using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.IO;

namespace Formats.Mobi.Headers
{
    public class PDBHeader
    {
        private long fileLen;
        private static readonly byte[] nullTwo = new byte[2];

        public int offset = 0x0;
        public readonly int baseLength = 0x4E;
        public int TotalLength
        {
            get => baseLength + (8 * (recordCount + 1)) + 2;
        }

        private string _title;
        public string title
        {
            get => _title;
            set
            {
                _title = value.Length > 0x20 ? value.Substring(0x0, 0x20) : value + new byte[0x20 - value.Length].Decode();

            }
        }
        public ushort attributes;
        public ushort version;
        public uint createdDate;
        public uint modifiedDate;
        public uint backupDate;
        public uint modificationNum;
        public uint appInfoId;
        public uint sortInfoID;
        public string type;
        public string creator;
        public uint uniqueIDseed;
        public uint nextRecordListID;
        public ushort recordCount;
        public uint[] records;

        /// <summary>
        /// Contains basic metadata for mobi including locations of other headers.
        /// </summary>
        public PDBHeader() { }

        public void FillDefault()
        {
            uint timestamp = Utils.Metadata.TimeStamp();
            title = "";
            attributes = 0;
            version = 1;
            createdDate = timestamp;
            modifiedDate = timestamp;
            backupDate = 0;
            modificationNum = 0;
            appInfoId = 0;
            sortInfoID = 0;
            type = "BOOK";
            creator = "MOBI";
            uniqueIDseed = (uint)Utils.Metadata.RandomNumber();
            nextRecordListID = 0;
            recordCount = 0;
            records = new uint[0];
        }

        public void Parse(BinaryReader reader)
        {
            fileLen = reader.BaseStream.Length;
            byte[] buffer;
            try
            {
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                buffer = reader.ReadBytes(baseLength);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to read PDB Header: {e.Message}");
            }

            title = buffer.SubArray(0x0, 0x20).Decode();
            attributes = Utils.BigEndian.ToUInt16(buffer, 0x20);
            version = Utils.BigEndian.ToUInt16(buffer, 0x22);
            createdDate = Utils.BigEndian.ToUInt32(buffer, 0x24);
            modifiedDate = Utils.BigEndian.ToUInt32(buffer, 0x28);
            backupDate = Utils.BigEndian.ToUInt32(buffer, 0x2C);
            modificationNum = Utils.BigEndian.ToUInt32(buffer, 0x30);
            appInfoId = Utils.BigEndian.ToUInt32(buffer, 0x34);
            sortInfoID = Utils.BigEndian.ToUInt32(buffer, 0x38);
            type = buffer.SubArray(0x3C, 0x4).Decode();
            creator = buffer.SubArray(0x40, 0x4).Decode();
            uniqueIDseed = Utils.BigEndian.ToUInt32(buffer, 0x44);
            nextRecordListID = Utils.BigEndian.ToUInt32(buffer, 0x48);
            recordCount = Utils.BigEndian.ToUInt16(buffer, 0x4C);

            try
            {
                reader.BaseStream.Seek(0x4E, SeekOrigin.Begin);
                buffer = reader.ReadBytes(0x8 * recordCount);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to read PDB records: {e.Message}");
            }

            if (recordCount < 2)
            {
                throw new FileFormatException("Invalid PDB record count.");
            }

            records = new uint[recordCount];
            for (var i = 0; i < recordCount; i++)
            {
                records[i] = Utils.BigEndian.ToUInt32(buffer, i * 0x8);
            }
        }

        public void DumpRecords(BinaryReader reader, string directory)
        {
            for (int i = 0; i < records.Length; i++)
            {
                uint start = records[i];
                uint end = i == records.Length - 1 ? records[i + 1] : (uint)reader.BaseStream.Length;

                uint len = end - start;
                reader.BaseStream.Seek(start, SeekOrigin.Begin);
                byte[] rec = reader.ReadBytes((int)len);

                File.WriteAllBytes(Path.Combine(directory, $"{i.ToString("D4")}.record"), rec);
            }
        }

        public byte[] Dump()
        {
            List<byte> output = new List<byte>();
            output.AddRange(title.Encode());

            output.AddRange(Utils.BigEndian.GetBytes(attributes));
            output.AddRange(Utils.BigEndian.GetBytes(version));
            output.AddRange(Utils.BigEndian.GetBytes(createdDate));
            output.AddRange(Utils.BigEndian.GetBytes(modifiedDate));
            output.AddRange(Utils.BigEndian.GetBytes(backupDate));

            output.AddRange(Utils.BigEndian.GetBytes(modificationNum));
            output.AddRange(Utils.BigEndian.GetBytes(appInfoId));
            output.AddRange(Utils.BigEndian.GetBytes(sortInfoID));
            output.AddRange(type.Encode());

            output.AddRange(creator.Encode());
            output.AddRange(Utils.BigEndian.GetBytes(uniqueIDseed));
            output.AddRange(Utils.BigEndian.GetBytes(nextRecordListID));
            output.AddRange(Utils.BigEndian.GetBytes((ushort)records.Length));

            for (int i = 0; i < records.Length; i++)
            {
                output.AddRange(Utils.BigEndian.GetBytes(records[i]));           // offset
                output.AddRange(Utils.BigEndian.GetBytes((i * 2) & 0x00FFFFFF)); // attr + uid
            }

            output.AddRange(nullTwo);

            return output.ToArray();
        }

        public void Write(BinaryWriter writer)
        {
            try
            {
                writer.BaseStream.Seek(this.offset, SeekOrigin.Begin);
                writer.Write(Dump());
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to write PDB header to file: {e.Message}");
            }
        }

        public uint RecordLength(uint recordNum)
        {
            if (recordNum < recordCount) return records[recordNum + 1] - records[recordNum];

            if (fileLen == 0)
            {
                throw new Exception("Cannot calculate length of last record -- file length is unknown");
            }

            if (records[recordNum] > fileLen)
            {
                throw new Exception("Record length extends past end of file");
            }
            return (uint)fileLen - records[recordNum];
        }

        public void Print()
        {
            Console.WriteLine($@"
PDBHeader:
    title: {title}
    attributes: {attributes}
    version: {version}
    created: {createdDate}
    modified: {modifiedDate}
    backup: {backupDate}
    modnum: {modificationNum}
    appInfoId: {appInfoId}
    sortInfoID: {sortInfoID}
    type: {type}
    creator: {creator}
    uniqueIDseed: {uniqueIDseed}
    nextRecordListID: {nextRecordListID}
    recordCount: {recordCount}
    Records:");
            for (int i = 0; i < records.Length; i++)
            {
                Console.WriteLine($"{i * 2}: ({records[i]}, 0)");
            }
        }
    }

}
