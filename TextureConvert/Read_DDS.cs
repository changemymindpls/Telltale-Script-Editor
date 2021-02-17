using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Telltale_Script_Editor.Utils;

namespace Telltale_Script_Editor.TextureConvert
{
    public class Read_DDS
    {
        //for byte utillities
        private ByteUtils byteUtils = new ByteUtils();

        /// <summary>
        /// The main function for reading a DDS file and parsing data from it.
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destinationFile"></param>
        public File_DDS Read_DDS_File(string sourceFilePath)
        {
            //read the source texture file into a byte array
            byte[] sourceFileData = File.ReadAllBytes(sourceFilePath);

            //initalize our variables for the dds header
            string texture_parsed_magic; //the magic "DDS "
            int texture_parsed_headerLength; //total byte size of the header data
            int texture_parsed_imageWidth; //size of the dds image pixel width
            int texture_parsed_imageHeight; //size of the dds image height height
            int texture_parsed_mipMapCount; //total amount of mip maps in the dds file
            string texture_parsed_compressionType; //compression type of the dds file

            //which byte offset we are on for the source file (will be changed as we go through the file)
            int file_bytePointerPosition = 0;

            //--------------------------1 DDS MAGIC--------------------------
            //start at the beginning
            file_bytePointerPosition = 0;

            //allocate 4 byte array (int32)
            byte[] texture_source_magic = byteUtils.AllocateBytes(4, sourceFileData, file_bytePointerPosition);

            //parse the byte array to int32
            texture_parsed_magic = Encoding.ASCII.GetString(texture_source_magic);

            //--------------------------2 DDS HEADER SIZE--------------------------
            //skip ahead to get the header size
            file_bytePointerPosition = 4;

            //allocate 4 byte array (int32)
            byte[] texture_source_headerLength = byteUtils.AllocateBytes(4, sourceFileData, file_bytePointerPosition);

            //parse the byte array to int32
            texture_parsed_headerLength = BitConverter.ToInt32(texture_source_headerLength, 0);

            //--------------------------3 DDS IMAGE HEIGHT--------------------------
            //skip ahead to the image height
            file_bytePointerPosition = 12;

            //allocate 4 byte array (int32)
            byte[] texture_source_imageHeight = byteUtils.AllocateBytes(4, sourceFileData, file_bytePointerPosition);

            //parse the byte array to int32
            texture_parsed_imageHeight = BitConverter.ToInt32(texture_source_imageHeight, 0);

            //--------------------------4 DDS IMAGE WIDTH--------------------------
            //skip ahead to the image width
            file_bytePointerPosition = 16;

            //allocate 4 byte array (int32)
            byte[] texture_source_imageWidth = byteUtils.AllocateBytes(4, sourceFileData, file_bytePointerPosition);

            //parse the byte array to int32
            texture_parsed_imageWidth = BitConverter.ToInt32(texture_source_imageWidth, 0);

            //--------------------------5 DDS MIP MAP COUNT--------------------------
            //skip ahead to the mip map count
            file_bytePointerPosition = 28;

            //allocate 4 byte array (int32)
            byte[] texture_source_mipMapCount = byteUtils.AllocateBytes(4, sourceFileData, file_bytePointerPosition);

            //parse the byte array to int32
            texture_parsed_mipMapCount = BitConverter.ToInt32(texture_source_mipMapCount, 0);

            //--------------------------6 DDS COMPRESSION TYPE--------------------------
            //note to self - be sure to get the pixel format header size as well later
            //skip ahead to get the compression type
            file_bytePointerPosition = 84;

            //allocate 4 byte array (int32)
            byte[] texture_source_compressionType = byteUtils.AllocateBytes(4, sourceFileData, file_bytePointerPosition);

            //parse the byte array to int32
            texture_parsed_compressionType = Encoding.ASCII.GetString(texture_source_compressionType);

            //--------------------------EXTRACT DDS TEXTURE DATA--------------------------
            //calculate dds header length (we add 4 because we skipped the 4 bytes which contain the dword, it isn't necessary to parse this data)
            int ddsHeaderLength = 4 + texture_parsed_headerLength;

            //calculate the length of just the dds texture data
            int ddsTextureDataLength = sourceFileData.Length - ddsHeaderLength;

            //allocate a byte array of dds texture length
            byte[] ddsTextureData = new byte[ddsTextureDataLength];

            //copy the data from the source byte array past the header (so we are only getting texture data)
            Array.Copy(sourceFileData, ddsHeaderLength, ddsTextureData, 0, ddsTextureData.Length);

            //--------------------------BUILD DDS FILE--------------------------
            //create our dds file object
            File_DDS file_DDS = new File_DDS();

            //assign the parsed values
            file_DDS.dwWidth = (uint)texture_parsed_imageWidth;
            file_DDS.dwHeight = (uint)texture_parsed_imageHeight;
            file_DDS.dwMipMapCount = (uint)texture_parsed_mipMapCount;
            file_DDS.ddspf_dwFourCC = texture_parsed_compressionType;
            file_DDS.textureData = ddsTextureData;
            file_DDS.filePath = sourceFilePath;

            //return the final object
            return file_DDS;
        }
    }
}
