using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Telltale_Script_Editor.Utils;

namespace Telltale_Script_Editor.TextureConvert
{
    //NOTE TO SELF: IF THERE IS AN EASIER WAY OF DOING THIS, DO IT SINCE THIS IS A RATHER LAZY WAY
    //DDS Docs - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-header
    //DDS PIXEL FORMAT - https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-pixelformat
    //DDS File Layout https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-file-layout-for-textures

    /// <summary>
    /// Main class for a DDS file
    /// </summary>
    public class File_DDS
    {
        //for byte utillities
        private ByteUtils byteUtils = new ByteUtils();

        //file path location
        public string filePath;

        //main dword [4 bytes]
        public readonly string dword = "DDS ";

        //--------------------------------------MAIN CHUNKS--------------------------------------
        //Size of structure. This member must be set to 124. [4 bytes]
        public readonly uint dwSize = 124;

        //Flags to indicate which members contain valid data. [4 bytes]
        public uint dwFlags = 528391;

        //Surface height (in pixels). [4 bytes]
        public uint dwHeight = 1024;

        //Surface width (in pixels). [4 bytes]
        public uint dwWidth = 1024;

        //The pitch or number of bytes per scan line in an uncompressed texture; the total number of bytes in the top level texture for a compressed texture. [4 bytes]
        public uint dwPitchOrLinearSize = 8192;

        //Depth of a volume texture (in pixels), otherwise unused. [4 bytes]
        public uint dwDepth = 0;

        //Number of mipmap levels, otherwise unused. [4 bytes]
        public uint dwMipMapCount = 0;

        //dds reserved bits
        //these are all unused according to DDS docs [all 4 bytes]
        public readonly uint dwReserved1 = 0;
        public readonly uint dwReserved2 = 0;
        public readonly uint dwReserved3 = 0;
        public readonly uint dwReserved4 = 0;
        public readonly uint dwReserved5 = 0;
        public readonly uint dwReserved6 = 0;
        public readonly uint dwReserved7 = 0;
        public readonly uint dwReserved8 = 0;
        public readonly uint dwReserved9 = 0;
        public readonly uint dwReserved10 = 0;
        public readonly uint dwReserved11 = 0;

        //--------------------------------------DDS PIXEL FORMAT STRUCT--------------------------------------
        //Structure size; set to 32 (bytes). [4 bytes]
        private readonly uint ddspf_size = 32;

        //Values which indicate what type of data is in the surface. [4 bytes]
        public uint ddspf_flags = 4;

        //Four-character codes for specifying compressed or custom formats. [4 bytes]
        //DXT1, DXT2, DXT3, DXT4, DXT5, etc.
        public string ddspf_dwFourCC = "DXT1";

        //Number of bits in an RGB (possibly including alpha) format. [4 bytes]
        public uint ddspf_rgbBitCount = 0;

        //Red (or lumiannce or Y) mask for reading color data. [4 bytes]
        public uint ddspf_RBitMask = 0;

        //Green (or U) mask for reading color data. [4 bytes]
        public uint ddspf_GBitMask = 0;

        //Blue (or V) mask for reading color data. [4 bytes]
        public uint ddspf_BBitMask = 0;

        //Alpha mask for reading alpha data. [4 bytes]
        public uint ddspf_ABitMask = 0;
        //--------------------------------------DDS PIXEL FORMAT STRUCT END--------------------------------------
        //--------------------------------------OTHER CHUNKS--------------------------------------

        //Specifies the complexity of the surfaces stored. [4 bytes]
        public uint dwCaps = 4096;

        //Additional detail about the surfaces stored. [4 bytes]
        public uint dwCaps2 = 0;

        //these 3 are unused according to DDS docs [all 4 bytes]
        public readonly uint dwCaps3 = 0;
        public readonly uint dwCaps4 = 0;
        public readonly uint dwReserved2_2 = 0;

        //--------------------------------------OTHER CHUNKS--------------------------------------

        //will contain the texture byte data after the dds header (with mip maps)
        public byte[] textureData;

        //total byte size of the mip maps (minus the largest one)
        public int mipMapByteSize;

        //--------------------------------------VARIABLES END--------------------------------------

        /// <summary>
        /// Manually builds a byte array of a dds header
        /// </summary>
        /// <returns></returns>
        public byte[] Build_DDSHeader_ByteArray()
        {
            //allocate our header byte array
            byte[] ddsHeader = new byte[128];

            //begin assigning our values main chunks
            ddsHeader = byteUtils.AllocateBytes_String(dword, ddsHeader, 0);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwSize), ddsHeader, 4);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwFlags), ddsHeader, 8);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwHeight), ddsHeader, 12);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwWidth), ddsHeader, 16);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwPitchOrLinearSize), ddsHeader, 20);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwDepth), ddsHeader, 24);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwMipMapCount), ddsHeader, 28);

            //write a bunch of unused chunks
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwReserved1), ddsHeader, 32);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwReserved2), ddsHeader, 36);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwReserved3), ddsHeader, 40);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwReserved4), ddsHeader, 44);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwReserved5), ddsHeader, 48);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwReserved6), ddsHeader, 52);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwReserved7), ddsHeader, 56);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwReserved8), ddsHeader, 60);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwReserved9), ddsHeader, 64);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwReserved10), ddsHeader, 68);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwReserved11), ddsHeader, 72);

            //write dds pixel format struct
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(ddspf_size), ddsHeader, 76);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(ddspf_flags), ddsHeader, 80);
            ddsHeader = byteUtils.AllocateBytes_String(ddspf_dwFourCC, ddsHeader, 84);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(ddspf_rgbBitCount), ddsHeader, 88);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(ddspf_RBitMask), ddsHeader, 92);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(ddspf_GBitMask), ddsHeader, 96);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(ddspf_BBitMask), ddsHeader, 100);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(ddspf_ABitMask), ddsHeader, 104);

            //write leftover main data
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwCaps), ddsHeader, 108);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwCaps2), ddsHeader, 112);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwCaps3), ddsHeader, 116);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwCaps4), ddsHeader, 120);
            ddsHeader = byteUtils.AllocateBytes(BitConverter.GetBytes(dwReserved2_2), ddsHeader, 124);

            //return the result
            return ddsHeader;
        }

        /// <summary>
        /// Parses the texture data from the dds, and reverses the orders of the mip maps (since the order is reversed in a d3dtx)
        /// </summary>
        /// <returns></returns>
        public byte[] Get_TextureData_Reversed()
        {
            //we will work through the texture data backwards, since the d3dtx format has mip map ordered reversed, so we will add it in that way

            //if there are no mip maps, go ahead and just build the texture
            if (dwMipMapCount <= 0)
            {
                //stop the function as there is no need to continue any further
                return textureData;
            }

            //offset for getting mip maps, we are working backwards since d3dtx has their mip maps stored backwards
            int leftoverOffset = textureData.Length;

            //allocate a byte array to contain our texture data (ordered backwards)
            byte[] textureData_reversed = new byte[0];

            //quick fix for textures not being read properly (they are not the exact same size)
            //note to self - try to modify the d3dtx header so the texture byte size in the header matches the texture byte size we are inputing
            //byte[] fillterData = new byte[8];
            //final_d3dtxData = Combine(final_d3dtxData, fillterData);

            //add 1 since the mip map count in dds files tend to start at 0 instead of 1 
            int mipCount = (int)dwMipMapCount + 1;

            //because I suck at math, we will generate our mip map resolutions using the same method we did in d3dtx to dds (can't figure out how to calculate them in reverse properly)
            int[,] mipImageResolutions = new int[mipCount, 2];

            //get our mip image dimensions (have to multiply by 2 as the mip calculations will be off by half)
            int mipImageWidth = (int)dwWidth * 2;
            int mipImageHeight = (int)dwHeight * 2;

            //add the resolutions in reverse
            for (int i = mipCount - 1; i > 0; i--)
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
            for (int i = 1; i < mipCount; i++)
            {
                //not required, just for viewing
                int mipLevel = mipCount - i;

                //get our mip resolution from the resolution array (the values are reversed, smallest to big)
                int width = mipImageResolutions[i, 0];
                int height = mipImageResolutions[i, 1];

                //estimate how many total bytes are in the largest texture mip level (main one)
                int byteSize_estimation = byteUtils.CalculateDDS_ByteSize(width, height, ddspf_dwFourCC.Equals("DXT1"));

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
                    Array.Copy(textureData, leftoverOffset, mipTexData, 0, mipTexData.Length);

                    //combine the new mip byte data to the existing texture data byte array
                    textureData_reversed = byteUtils.CombineByteArray(textureData_reversed, mipTexData);
                }

                //write results to the console for viewing
                Console.WriteLine("Leftover Offset = {0}", leftoverOffset.ToString());
                Console.WriteLine("Mip Level = {0}", mipLevel.ToString());
                Console.WriteLine("Mip Resolution = {0}x{1}", width.ToString(), height.ToString());
                Console.WriteLine("Mip Level Byte Size = {0}", byteSize_estimation.ToString());
                Console.WriteLine("D3DTX Data Length Byte Size = {0}", textureData_reversed.Length.ToString());
            }

            //return the final reversed texture data
            return textureData_reversed;
        }

        /// <summary>
        /// Builds the DDS file (with the header) into a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] Build_DDS_File()
        {
            //build the dds header
            byte[] ddsHeader = Build_DDSHeader_ByteArray();

            //combine and build the final byte array for the file
            byte[] totalBytes = byteUtils.CombineByteArray(ddsHeader, textureData);

            //return the final byte array
            return totalBytes;
        }
    }
}
