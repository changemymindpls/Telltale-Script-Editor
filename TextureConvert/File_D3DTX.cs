using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Telltale_Script_Editor.Utils;

namespace Telltale_Script_Editor.TextureConvert
{
    /// <summary>
    /// Main class for a D3DTX file.
    /// </summary>
    public class File_D3DTX
    {
        //for byte utillity functions
        private ByteUtils byteUtils = new ByteUtils();

        //file path location
        public string filePath;

        //our byte arrays containing the bulk of data from the d3dtx file
        //just the header data
        public byte[] headerData;
        public byte[] textureData;
        //all the texture data
        public byte[] textureData_reOrdered;
        //the texture data with re-ordered mip maps (used for D3DTX --> DDS since mip maps are ordered backwards)

        //main variables for a d3dtx file
        //the name of the original file + extension
        public string originalFileName;

        //magic string indicating what file it is
        public string magic;

        //total byte size of the texture data, used to calculate the header length
        public int textureDataByteSize;

        //the length of the file name string in the header
        public int fileNameInFileLength;

        //the parsed file name from the header
        public string fileNameInFile_string;

        //image mip map count
        public int imageMipMapCount;

        //main image pixel width
        public int imageWidth;

        //main image pixel height
        public int imageHeight;

        //dxt type?
        public int dxtType;

        //length of the telltale d3dtx header
        public int headerLength;

        //total byte size of the mip maps (minus the largest one)
        public int mipMapByteSize;

        //not required to be in a D3DTX file but helpful
        //dxt type as a string
        public string parsed_dxtType_string;

        //image mip map count - 1 (since images with no mip maps have this value set to 1)
        public int imageMipMapCount_decremented;

        /// <summary>
        /// Manually builds a byte array of a d3dtx header
        /// </summary>
        /// <returns></returns>
        public byte[] Build_D3DTXHeader_ByteArray()
        {
            //pointer for indicating what byte we are on in the file
            int headerBytePointerPosition = 0;

            //begin assigning our values main chunks

            //skip to the textureDataByteSize and set our new textureDataByteSize
            headerBytePointerPosition = 12;
            headerData = byteUtils.ModifyBytes(headerData, BitConverter.GetBytes(textureDataByteSize), headerBytePointerPosition);

            //skip to the fileNameInFileLength and set our new fileNameInFileLength
            //headerBytePointerPosition = 124;
            //headerData = byteUtils.ModifyBytes(headerData, BitConverter.GetBytes(fileNameInFileLength), headerBytePointerPosition);

            //skip to the fileNameInFile_string and convert the file name in file to a byte array and set it
            //headerBytePointerPosition = 128;

            //byte[] fileNameInFile_string_byteArray = new byte[fileNameInFile_string.Length];
            //fileNameInFile_string_byteArray = byteUtils.AllocateBytes_String(fileNameInFile_string, fileNameInFile_string_byteArray, 0);

            //headerData = byteUtils.ModifyBytes(headerData, fileNameInFile_string_byteArray, headerBytePointerPosition);
            //headerBytePointerPosition += fileNameInFile_string.Length;

            //skip to the mip map count and set the value
            headerBytePointerPosition += 132 + fileNameInFile_string.Length + 13;
            headerData = byteUtils.ModifyBytes(headerData, BitConverter.GetBytes(imageMipMapCount), headerBytePointerPosition);

            //skip to the image width and set the value
            headerBytePointerPosition += 4;
            headerData = byteUtils.ModifyBytes(headerData, BitConverter.GetBytes(imageWidth), headerBytePointerPosition);

            //skip to the image height and set the value
            headerBytePointerPosition += 4;
            headerData = byteUtils.ModifyBytes(headerData, BitConverter.GetBytes(imageHeight), headerBytePointerPosition);

            //skip to the dxt type and set the value
            headerBytePointerPosition += 12;
            headerData = byteUtils.ModifyBytes(headerData, BitConverter.GetBytes(dxtType), headerBytePointerPosition);

            //skip to the mipMapByteSize
            headerBytePointerPosition = headerData.Length - 4;

            //modify mip map byte size
            headerData = byteUtils.ModifyBytes(headerData, BitConverter.GetBytes(mipMapByteSize), headerBytePointerPosition);

            //return the result
            return headerData;
        }

        public int GetDXT_String(string keyword)
        {
            //special cases
            if (parsed_dxtType_string.Equals("DXT5"))
            {
                //DXT5 COMPRESSION
                return 66;
            }
            else if (parsed_dxtType_string.Equals("BC5U"))
            {
                //DDSPF_BC5_UNORM COMPRESSION
                return 68;
            }
            else if (parsed_dxtType_string.Equals("BC4U"))
            {
                //DDSPF_BC4_UNORM COMPRESSION
                return 67;
            }
            else
            {
                //DXT1 default
                return 64;
            }
        }

        public byte[] Build_D3DTX_File_FromDDSAndHeader(File_DDS file_DDS, File_D3DTX file_D3DTX_header)
        {
            /*
             * NOTE TO SELF
             * DDS --> D3DTX EXTRACTION, THE BYTES ARE NOT FULLY 1:1 WHEN THERE IS A CONVERSION (off by 8 bytes)
             * MABYE TRY TO CHANGE THE TEXTURE DATA BYTE SIZE IN THE D3DTX HEADER AND SEE IF THAT CHANGES ANYTHING
            */

            //read the source texture file into a byte array
            //byte[] sourceTexFileData = File.ReadAllBytes(sourceTexFile);
            byte[] sourceTexFileData = file_DDS.textureData;

            //read the source header file into a byte array
           // byte[] sourceHeaderFileData = File.ReadAllBytes(sourceHeaderFile);
            byte[] sourceHeaderFileData = file_D3DTX_header.headerData;

            string sourceHeaderFileName = Path.GetFileName(file_D3DTX_header.filePath);
            string sourceHeaderFileName_newExt = sourceHeaderFileName.Remove(sourceHeaderFileName.Length - Path.GetExtension(file_D3DTX_header.filePath).Length, Path.GetExtension(file_D3DTX_header.filePath).Length);
            sourceHeaderFileName_newExt += ".d3dtx";

            //get our file name and convert it to a byte array (since d3dtx has the filename.extension written in the file)
            byte[] fileNameBytes = Encoding.ASCII.GetBytes(sourceHeaderFileName_newExt); //currently unused (but will be)

            //write the result to the console for viewing
            Console.WriteLine("Total Source Texture Byte Size = {0}", sourceTexFileData.Length);

            //write the result to the console for viewing
            Console.WriteLine("Total Source Header Byte Size = {0}", sourceHeaderFileData.Length);

            //which byte offset we are on for the source texture (will be changed as we go through the file)
            int texture_bytePointerPosition = 0;

            //which byte offset we are on for the source header (will be changed as we go through the file)
            int header_bytePointerPosition = 0; //currently not used (but will be when we start modifying the d3dtx header)

            //--------------------------EXTRACT DDS TEXTURE DATA--------------------------
            //calculate dds header length (we add 4 because we skipped the 4 bytes which contain the dword, it isn't necessary to parse this data)
            int ddsHeaderLength = 4 + (int)file_DDS.dwSize;

            //calculate the length of just the dds texture data
            int ddsTextureDataLength = sourceTexFileData.Length - ddsHeaderLength;

            //allocate a byte array of dds texture length
            byte[] ddsTextureData = new byte[ddsTextureDataLength];

            //copy the data from the source byte array past the header (so we are only getting texture data)
            Array.Copy(sourceTexFileData, ddsHeaderLength, ddsTextureData, 0, ddsTextureData.Length);

            //--------------------------COMBINE DDS TEXTURE DATA WITH D3DTX HEADER--------------------------
            int total_d3dtxLength = ddsTextureData.Length + sourceHeaderFileData.Length;

            //change the items in the header based on the d3dtx
            file_D3DTX_header.imageHeight = (int)file_DDS.dwHeight;
            file_D3DTX_header.imageWidth = (int)file_DDS.dwWidth;
            //file_D3DTX_header.imageMipMapCount = (int)file_DDS.dwMipMapCount;
            file_D3DTX_header.dxtType = GetDXT_String(file_DDS.ddspf_dwFourCC);
            file_D3DTX_header.parsed_dxtType_string = file_DDS.ddspf_dwFourCC;

            //if there are no mip maps, go ahead and just build the texture
            if (file_DDS.dwMipMapCount < 1)
            {
                //write the data to the file, combine the generted DDS header and our new texture byte data
                //stop the function as there is no need to continue any further
                return byteUtils.CombineByteArray(sourceHeaderFileData, ddsTextureData);
            }

            //we will work through the texture data backwards, since the d3dtx format has mip map ordered reversed, so we will add it in that way
            //offset for getting mip maps, we are working backwards since d3dtx has their mip maps stored backwards
            int leftoverOffset = ddsTextureData.Length;

            //allocate a byte array to contain our texture data (ordered backwards)
            byte[] final_d3dtxData = new byte[0];

            //modify the texture file size data in the header
            sourceHeaderFileData = byteUtils.ModifyBytes(sourceHeaderFileData, BitConverter.GetBytes(ddsTextureData.Length), 12);

            //add the d3dtx header
            //note to self - modify the header before adding it
            final_d3dtxData = byteUtils.CombineByteArray(sourceHeaderFileData, final_d3dtxData);

            //quick fix for textures not being read properly (they are not the exact same size)
            //note to self - try to modify the d3dtx header so the texture byte size in the header matches the texture byte size we are inputing
            //byte[] fillterData = new byte[8];
            //final_d3dtxData = Combine(final_d3dtxData, fillterData);

            //add 1 since the mip map count in dds files tend to start at 0 instead of 1 
            int mipMaps = (int)file_DDS.dwMipMapCount + 1;
            int imageWidth = (int)file_DDS.dwWidth;
            int imageHeight = (int)file_DDS.dwHeight;

            //because I suck at math, we will generate our mip map resolutions using the same method we did in d3dtx to dds (can't figure out how to calculate them in reverse properly)
            int[,] mipImageResolutions = new int[mipMaps, 2];

            //get our mip image dimensions (have to multiply by 2 as the mip calculations will be off by half)
            int mipImageWidth = imageWidth * 2;
            int mipImageHeight = imageHeight * 2;

            //add the resolutions in reverse
            for (int i = mipMaps - 1; i > 0; i--)
            {
                //divide the resolutions by 2
                mipImageWidth /= 2;
                mipImageHeight /= 2;

                //assign the resolutions
                mipImageResolutions[i, 0] = mipImageWidth;
                mipImageResolutions[i, 1] = mipImageHeight;
            }

            //not required, just for viewing
            int totalMipByteSize = 0;

            //run a loop for the amount of mip maps
            for (int i = 1; i < mipMaps; i++)
            {
                //not required, just for viewing
                int mipLevel = mipMaps - i;

                //get our mip resolution from the resolution array (the values are reversed, smallest to big)
                int width = mipImageResolutions[i, 0];
                int height = mipImageResolutions[i, 1];

                //estimate how many total bytes are in the largest texture mip level (main one)
                int byteSize_estimation = byteUtils.CalculateDDS_ByteSize(width, height, file_DDS.ddspf_dwFourCC.Equals("DXT1"));

                if (i != 1)
                    mipMapByteSize += byteSize_estimation;

                //offset our variable so we can get to the next mip (we are working backwards from the end of the file)
                leftoverOffset -= byteSize_estimation;

                //not required, just for viewing
                totalMipByteSize += byteSize_estimation;

                //allocate a byte array with the estimated byte size
                byte[] mipTexData = new byte[byteSize_estimation];

                //check to see if we are not over the length of the file (we are working backwards)
                if (leftoverOffset >= 0)
                {
                    //copy all the bytes from the texture byte array after the leftoverOffset, and copy that data to the mip map tex data byte array
                    Array.Copy(ddsTextureData, leftoverOffset, mipTexData, 0, mipTexData.Length);

                    //combine the new mip byte data to the existing texture data byte array
                    final_d3dtxData = byteUtils.CombineByteArray(final_d3dtxData, mipTexData);
                }

                //write results to the console for viewing
                Console.WriteLine("Leftover Offset = {0}", leftoverOffset.ToString());
                Console.WriteLine("Mip Level = {0}", mipLevel.ToString());
                Console.WriteLine("Mip Resolution = {0}x{1}", width.ToString(), height.ToString());
                Console.WriteLine("Mip Level Byte Size = {0}", byteSize_estimation.ToString());
                Console.WriteLine("D3DTX Data Length Byte Size = {0}", final_d3dtxData.Length.ToString());
            }

            //write results to the console for viewing
            Console.WriteLine("D3DTX Header Byte Size = {0}", sourceHeaderFileData.Length.ToString());

            //return our final byte array
            return final_d3dtxData;
        }

        /// <summary>
        /// Builds the D3DTX file (with the header) into a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] Build_D3DTX_File()
        {
            //build the dds header
            byte[] d3dtxHeader = Build_D3DTXHeader_ByteArray();

            //combine and build the final byte array for the file
            byte[] totalBytes = byteUtils.CombineByteArray(d3dtxHeader, textureData);

            //return the final byte array
            return totalBytes;
        }
    }
}
