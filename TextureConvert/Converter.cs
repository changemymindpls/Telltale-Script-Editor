using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Telltale_Script_Editor.FileManagement;
using Telltale_Script_Editor.Utils;

namespace Telltale_Script_Editor.TextureConvert
{
    public class Converter
    {
        private IOManagement ioManagement = new IOManagement();
        private ByteUtils byteUtils = new ByteUtils();

        public void Convert_DDS_To_D3DTX(string ddsFilePath, string d3dtx_header_FilePath, bool removeOriginals = false)
        {
            //get our readers
            Read_DDS read_DDS = new Read_DDS();
            Read_D3DTX read_D3DTX = new Read_D3DTX();

            //read the dds and header file
            File_DDS file_DDS = read_DDS.Read_DDS_File(ddsFilePath);
            File_D3DTX file_D3DTX_header = read_D3DTX.Read_D3DTX_File(d3dtx_header_FilePath, true);

            //build our final d3dtx file path
            string finalD3DTX_path = ddsFilePath.Replace(".dds", ".d3dtx");

            //assign the modified adata
            file_D3DTX_header.imageHeight = (int)file_DDS.dwHeight;
            file_D3DTX_header.imageWidth = (int)file_DDS.dwWidth;
            file_D3DTX_header.imageMipMapCount_decremented = (int)file_DDS.dwMipMapCount;
            file_D3DTX_header.textureDataByteSize = file_DDS.textureData.Length;
            file_D3DTX_header.dxtType = file_D3DTX_header.GetDXT_String(file_DDS.ddspf_dwFourCC);
            file_D3DTX_header.parsed_dxtType_string = file_DDS.ddspf_dwFourCC;

            //build the final d3dtx file in a byte array, combine the texture data and the header
            byte[] finalFile = byteUtils.CombineByteArray(file_D3DTX_header.Build_D3DTXHeader_ByteArray(), file_DDS.Get_TextureData_Reversed());

            //write to disk
            File.WriteAllBytes(finalD3DTX_path, finalFile);

            //remove the original files
            if (removeOriginals)
            {
                ioManagement.DeleteFile(ddsFilePath);
                ioManagement.DeleteFile(d3dtx_header_FilePath);
            }
        }

        public void Convert_D3DTX_To_DDS(string filePath, bool removeOriginal = false)
        {
            //get our d3dtx reader and parse the d3dtx file
            Read_D3DTX read_D3DTX = new Read_D3DTX();
            File_D3DTX file_D3DTX = read_D3DTX.Read_D3DTX_File(filePath);

            //get our file paths for the new dds file and the header file
            string finalDDS_path = filePath.Replace(".d3dtx", ".dds");
            string finalHeader_path = filePath.Replace(".d3dtx", ".header");

            //write the header data to the disk
            File.WriteAllBytes(finalHeader_path, file_D3DTX.headerData);

            //build the dds file
            File_DDS file_DDS = new File_DDS();

            //assign the texture data from the parsed d3dtx (the reversed since data is reversed in a d3dtx)
            file_DDS.textureData = file_D3DTX.textureData;

            //assign the values
            file_DDS.dwWidth = (uint)file_D3DTX.imageWidth;
            file_DDS.dwHeight = (uint)file_D3DTX.imageHeight;
            file_DDS.dwMipMapCount = (uint)file_D3DTX.imageMipMapCount_decremented;
            file_DDS.filePath = finalDDS_path;
            file_DDS.ddspf_dwFourCC = file_D3DTX.parsed_dxtType_string;

            //write the file dds to file
            File.WriteAllBytes(finalDDS_path, file_DDS.Build_DDS_File());

            //remove the original files
            if (removeOriginal)
                ioManagement.DeleteFile(filePath);
        }
    }
}
