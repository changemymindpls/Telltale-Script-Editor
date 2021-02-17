using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Telltale_Script_Editor.Utils;

namespace Telltale_Script_Editor.TextureConvert
{
    public class Read_D3DTX
    {
        //for byte utillities
        private ByteUtils byteUtils = new ByteUtils();

        /// <summary>
        /// The main function for reading and parsing a d3dtx file and it's data.
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destinationFile"></param>
        public File_D3DTX Read_D3DTX_File(string sourceFile, bool headerOnly = false)
        {
            //just for display
            Console.WriteLine("Opening Image {0}", sourceFile);

            //get our d3dtx object ready for later use.
            File_D3DTX file_D3DTX;

            //read the source file into a byte array
            byte[] sourceByteFile = File.ReadAllBytes(sourceFile);

            //get the file name
            string sourceFileName = Path.GetFileName(sourceFile);

            //path
            string sourceFilePath_replaced = sourceFile;

            //get our file name and convert it to a byte array (since d3dtx has the filename.extension written in the file)
            byte[] fileNameBytes = Encoding.ASCII.GetBytes(sourceFileName);

            string headerExtension = ".header";

            //if the source file has a .header extnesion then replace the extension with .d3dtx (because in the d3dtx header, the name is stored with the .d3dtx extension)
            if(Path.GetExtension(sourceFilePath_replaced).Equals(headerExtension))
            {
                //set file path
                string newSourceFileName = Path.GetFileName(sourceFile).Remove(sourceFileName.Length - headerExtension.Length, headerExtension.Length);
                sourceFilePath_replaced = string.Format("{0}.d3dtx", newSourceFileName);

                //set file name with extension
                string newName = sourceFileName.Remove(sourceFileName.Length - headerExtension.Length, headerExtension.Length);
                sourceFileName = string.Format("{0}.d3dtx", newName);

                //convert the new name to bytes
                fileNameBytes = Encoding.ASCII.GetBytes(sourceFileName);
            }

            //our byte arrays containing the bulk of data from the d3dtx file
            byte[] headerData; //just the header data
            byte[] textureData; //all the texture data
            byte[] textureData_reOrdered; //the texture data with re-ordered mip maps

            //initalize our variables, this data will be parsed and assigned
            string parsed_dword; //magic dword
            int parsed_textureDataByteSize; //total byte size of the texture data, used to calculate the header length
            int parsed_imageMipMapCount; //image mip map count
            int parsed_imageMipMapCount_decremented; //image mip map count - 1 (since images with no mip maps have this value set to 1)
            int parsed_imageWidth; //main image pixel width
            int parsed_imageHeight; //main image pixel height
            int parsed_dxtType; //dxt type?
            int headerLength; //length of the telltale d3dtx header
            int parsed_mipMapByteSize;
            string parsed_dxtType_string; //dxt type as a string
            string parsed_fileNameInFile_string; //the parsed file name from the header
            int parsed_fileNameInFileLength; //the length of the file name string in the header

            //which byte offset we are on (will be changed as we go through the file)
            int bytePointerPosition = 0;

            //--------------------------1 = DWORD--------------------------
            //offset byte pointer location to get the DWORD
            bytePointerPosition = 0;

            //allocate 4 byte array (string)
            byte[] source_dword = byteUtils.AllocateBytes(4, sourceByteFile, bytePointerPosition);

            //parse the byte array to string
            parsed_dword = Encoding.ASCII.GetString(source_dword);

            //write the result to the console for viewing
            Console.WriteLine("DWORD = {0}", parsed_dword);
            //--------------------------2 = TEXTURE BYTE SIZE--------------------------
            //offset byte pointer location to get the TEXTURE BYTE SIZE
            bytePointerPosition = 12;

            //allocate 4 byte array (int32)
            byte[] source_fileSize = byteUtils.AllocateBytes(4, sourceByteFile, bytePointerPosition);

            //parse the byte array to int32
            parsed_textureDataByteSize = BitConverter.ToInt32(source_fileSize, 0);

            //calculating header length, parsed texture byte size - source byte size
            if(Path.GetExtension(sourceFile).Equals(".header"))
                headerLength = sourceByteFile.Length;
            else
                headerLength = sourceByteFile.Length - parsed_textureDataByteSize;

            //write the result to the console for viewing
            Console.WriteLine("Texture Byte Size = {0}", parsed_textureDataByteSize.ToString());
            Console.WriteLine("Header Byte Size = {0}", headerLength.ToString());
            //--------------------------3 = SCREWY TELLTALE DATA--------------------------
            //NOTE TO SELF - no need to parse this byte data, we can extract the entire header later with this info included and just change what we need

            //offset byte pointer location to get the SCREWY TELLTALE DATA
            bytePointerPosition = 20;

            int telltaleScrewyHeaderLength = 84; //screwy header offset length

            byte[] screwyHeaderData = new byte[telltaleScrewyHeaderLength];

            for (int i = 0; i < telltaleScrewyHeaderLength; i++)
            {
                screwyHeaderData[i] = sourceByteFile[bytePointerPosition + i];
            }

            //move the pointer past the screwy header
            bytePointerPosition += telltaleScrewyHeaderLength;

            //--------------------------4 = TEXTURE FILE NAME--------------------------
            //offset byte pointer location to get the TEXTURE FILE NAME

            //if(headerOnly)
            //    bytePointerPosition += 20;
            //else
                bytePointerPosition += 24;

            //allocate 4 byte array (int32)
            byte[] source_fileNameInFileLength = byteUtils.AllocateBytes(4, sourceByteFile, bytePointerPosition);

            //parse the byte array to int32
            parsed_fileNameInFileLength = BitConverter.ToInt32(source_fileNameInFileLength, 0);

            //skip 4 bytes ahead
            bytePointerPosition += 4;

            //---------check for matching bytes
            //even if the bytes dont match we are parsing the data (since a user alerted me of a case where the file names DONT match
            byte[] source_fileNameInFile = byteUtils.AllocateBytes(parsed_fileNameInFileLength, sourceByteFile, bytePointerPosition);

            //parse the byte array to string
            parsed_fileNameInFile_string = Encoding.ASCII.GetString(source_fileNameInFile);

            //if the bytes don't match 100%, we can't convert because our offsets later on will be off!
            if (!(fileNameBytes.Length != parsed_fileNameInFileLength))
            {
                Console.WriteLine("WARNING, the file name in the header '{0}' is different than the actual file name '{1}'. Proceeding anyway.", parsed_fileNameInFile_string, sourceFileName);

                //return null; //return the method and don't continue any further
            }

            //move the cursor past the filename.extension byte string
            //bytePointerPosition += fileNameBytes.Length;
            bytePointerPosition += parsed_fileNameInFileLength;

            //--------------------------5 = MIP MAP COUNT--------------------------
            //offset byte pointer location to get the MIP MAP COUNT
            bytePointerPosition += 13;

            //allocate 4 byte array (int32)
            byte[] source_imageMipMapCount = byteUtils.AllocateBytes(4, sourceByteFile, bytePointerPosition);

            //parse the byte array to int32
            parsed_imageMipMapCount = BitConverter.ToInt32(source_imageMipMapCount, 0);
            parsed_imageMipMapCount_decremented = parsed_imageMipMapCount - 1;

            //write the result to the console for viewing
            Console.WriteLine(string.Format("Mip Map Count = {0} ({1})", parsed_imageMipMapCount.ToString(), parsed_imageMipMapCount_decremented.ToString()));
            //--------------------------6 = MAIN IMAGE WIDTH--------------------------
            //offset byte pointer location to get the MAIN IMAGE WIDTH
            bytePointerPosition += 4;

            //allocate 4 byte array (int32)
            byte[] source_imageWidth = byteUtils.AllocateBytes(4, sourceByteFile, bytePointerPosition);

            //parse the byte array to int32
            parsed_imageWidth = BitConverter.ToInt32(source_imageWidth, 0);

            //write the result to the console for viewing
            Console.WriteLine("Image Width = {0}", parsed_imageWidth.ToString());
            //--------------------------7 = MAIN IMAGE HEIGHT--------------------------
            //offset byte pointer location to get the MAIN IMAGE HEIGHT
            bytePointerPosition += 4;

            //allocate 4 byte array (int32)
            byte[] source_imageHeight = byteUtils.AllocateBytes(4, sourceByteFile, bytePointerPosition);

            //parse the byte array to int32
            parsed_imageHeight = BitConverter.ToInt32(source_imageHeight, 0);

            //write the result to the console for viewing
            Console.WriteLine("Image Height = {0}", parsed_imageHeight.ToString());
            //--------------------------8 = DXT TYPE--------------------------
            //offset byte pointer location to get the DXT TYPE
            bytePointerPosition += 12;

            //allocate 4 byte array (int32)
            byte[] source_dxtType = byteUtils.AllocateBytes(4, sourceByteFile, bytePointerPosition);

            //parse the byte array to int32
            parsed_dxtType = BitConverter.ToInt32(source_dxtType, 0);

            //write the result to the console for viewing
            Console.WriteLine("DXT TYPE = {0}", parsed_dxtType.ToString());
            //--------------------------9 = MIP MAP BYTE SIZE--------------------------
            //offset byte pointer location to get the DXT TYPE
            bytePointerPosition = headerLength - 4;

            //allocate 4 byte array (int32)
            byte[] source_mipMapByteSize = byteUtils.AllocateBytes(4, sourceByteFile, bytePointerPosition);

            //parse the byte array to int32
            parsed_mipMapByteSize = BitConverter.ToInt32(source_mipMapByteSize, 0);

            //write the result to the console for viewing
            Console.WriteLine("Mip Map Byte Size = {0}", parsed_mipMapByteSize.ToString());
            //--------------------------GETTING COMPRESSION TYPE--------------------------
            //NOTE TO SELF 1 - BIGGEST ISSUE RIGHT NOW IS MIP MAPS, NEED TO PARSE MORE INFORMATION FROM D3DTX TO BE ABLE TO EXTRACT MIP MAPS PROPERLY
            //NOTE TO SELF 2 - largest mip map (main texture) extraction is successful, the next step is getting the lower mip levels seperately, thank you microsoft for the byte size calculation forumla
            //NOTE TO SELF 3 - all mip maps can be extracted sucessfully and implemented into the DDS file
            //SET DDS COMPRESSION TYPES

            //default
            parsed_dxtType_string = "DXT1";

            //special cases
            if (parsed_dxtType == 66)
            {
                //DXT5 COMPRESSION
                parsed_dxtType_string = "DXT5";
            }
            else if (parsed_dxtType == 68)
            {
                //DDSPF_BC5_UNORM COMPRESSION
                parsed_dxtType_string = "BC5U";
            }
            else if (parsed_dxtType == 67)
            {
                //DDSPF_BC4_UNORM COMPRESSION
                parsed_dxtType_string = "BC4U";
            }
            //else if (parsed_compressionType == 67)
            //{
            //DDSPF_DXT3 COMPRESSION
            //    dds_File.ddspf_dwFourCC = "DXT3";
            //}

            //--------------------------GET D3DTX HEADER--------------------------
            //getting this data so we can generate a .header file that will accompany the .dds on conversion, this .header file will contain the original .d3dtx header for converting the .dds back later

            //if we are reading only the header of this texture, then fill our new object up and stop.
            if(headerOnly)
            {
                //allocate a byte array to contain the header data
                headerData = new byte[sourceByteFile.Length];

                //copy all the bytes from the source byte file after the header length, and copy that data to the texture data byte array
                Array.Copy(sourceByteFile, 0, headerData, 0, headerData.Length);

                //create our d3dtx object
                file_D3DTX = new File_D3DTX();

                //assign our values
                file_D3DTX.magic = parsed_dword;
                file_D3DTX.textureDataByteSize = parsed_textureDataByteSize;
                file_D3DTX.imageHeight = parsed_imageHeight;
                file_D3DTX.imageWidth = parsed_imageWidth;
                file_D3DTX.imageMipMapCount = parsed_imageMipMapCount;
                file_D3DTX.imageMipMapCount_decremented = parsed_imageMipMapCount_decremented;
                file_D3DTX.dxtType = parsed_dxtType;
                file_D3DTX.parsed_dxtType_string = parsed_dxtType_string;
                file_D3DTX.originalFileName = sourceFileName;
                file_D3DTX.fileNameInFileLength = parsed_fileNameInFileLength;
                file_D3DTX.fileNameInFile_string = parsed_fileNameInFile_string;
                file_D3DTX.headerLength = headerLength;
                file_D3DTX.filePath = sourceFile;
                file_D3DTX.mipMapByteSize = parsed_mipMapByteSize;

                //assign the main chunks of byte data
                file_D3DTX.headerData = headerData;

                //we are done!
                return file_D3DTX;
            }

            //allocate a byte array to contain the header data
            headerData = new byte[headerLength];

            //copy all the bytes from the source byte file after the header length, and copy that data to the texture data byte array
            Array.Copy(sourceByteFile, 0, headerData, 0, headerData.Length);

            //--------------------------GET TEXTURE DATA FROM D3DTX--------------------------
            //estimate how many total bytes are in the largest texture mip level (main one)
            int mainTextureByteSize_Estimation = byteUtils.CalculateDDS_ByteSize(parsed_imageWidth, parsed_imageHeight, parsed_dxtType == 64);

            //write the result to the console for viewing
            Console.WriteLine("calculated Largest Mip Level Byte Size = {0}", mainTextureByteSize_Estimation.ToString());

            //initalize our start offset, this is used to offset the array copy
            int startOffset;

            //if our estimation is not accurate, then just extract the whole fuckin thing
            if (mainTextureByteSize_Estimation > parsed_textureDataByteSize)
            {
                //offset the byte pointer position just past the header
                startOffset = headerLength;

                //allocate byte array with the parsed length of the total texture byte data from the header
                textureData = new byte[parsed_textureDataByteSize];
            }
            else
            {
                //if our estimation is accurate, then we will attempt to extract the largest mip map (we are starting from the end of the file since the largest mip is stored at the end)

                //offset the byte pointer position just past the header
                startOffset = sourceByteFile.Length - mainTextureByteSize_Estimation;

                //calculate main texture level
                textureData = new byte[mainTextureByteSize_Estimation];
            }

            //copy all the bytes from the source byte file after the header length, and copy that data to the texture data byte array
            Array.Copy(sourceByteFile, startOffset, textureData, 0, textureData.Length);

            //if there are no mip maps, build the texture file because we are done (there is only one mip map and we already extracted it)
            if (parsed_imageMipMapCount <= 1)
            {
                //create our d3dtx object
                file_D3DTX = new File_D3DTX();

                //assign our values
                file_D3DTX.magic = parsed_dword;
                file_D3DTX.textureDataByteSize = parsed_textureDataByteSize;
                file_D3DTX.imageHeight = parsed_imageHeight;
                file_D3DTX.imageWidth = parsed_imageWidth;
                file_D3DTX.imageMipMapCount = parsed_imageMipMapCount;
                file_D3DTX.imageMipMapCount_decremented = parsed_imageMipMapCount_decremented;
                file_D3DTX.dxtType = parsed_dxtType;
                file_D3DTX.parsed_dxtType_string = parsed_dxtType_string;
                file_D3DTX.originalFileName = sourceFileName;
                file_D3DTX.fileNameInFileLength = parsed_fileNameInFileLength;
                file_D3DTX.fileNameInFile_string = parsed_fileNameInFile_string;
                file_D3DTX.headerLength = headerLength;
                file_D3DTX.filePath = sourceFile;
                file_D3DTX.mipMapByteSize = parsed_mipMapByteSize;

                //assign the main chunks of byte data
                file_D3DTX.headerData = headerData;
                file_D3DTX.textureData = textureData;

                //we are done!
                return file_D3DTX;
            }

            //--------------------------MIP MAP EXTRACTION AND BUILDING--------------------------
            //offset for getting mip maps, we are working backwards since d3dtx has their mip maps stored backwards
            int leftoverOffset = sourceByteFile.Length - mainTextureByteSize_Estimation;

            textureData_reOrdered = textureData;

            //get image mip dimensions (will be modified when the loop is iterated)
            int mipImageWidth = parsed_imageWidth;
            int mipImageHeight = parsed_imageHeight;

            //not required, just for viewing
            int totalMipByteSize = 0;

            //run a loop for the amount of mip maps
            for (int i = 1; i < parsed_imageMipMapCount; i++)
            {
                //write the result to the console for viewing
                Console.WriteLine("Mip Level = {0}", i.ToString());

                //divide the dimensions by 2 when stepping down on each mip level
                mipImageWidth /= 2;
                mipImageHeight /= 2;

                //write the result to the console for viewing
                Console.WriteLine("Mip Resolution = {0}x{1}", mipImageWidth.ToString(), mipImageHeight.ToString());

                //estimate how many total bytes are in the largest texture mip level (main one)
                int byteSize_estimation = byteUtils.CalculateDDS_ByteSize(mipImageWidth, mipImageHeight, parsed_dxtType == 64);
                //offset our variable so we can get to the next mip (we are working backwards from the end of the file)
                leftoverOffset -= byteSize_estimation;

                //not required, just for viewing
                totalMipByteSize += byteSize_estimation;

                //write the result to the console for viewing
                Console.WriteLine("Mip Level Byte Size = {0}", byteSize_estimation.ToString());

                //allocate a byte array with the estimated byte size
                byte[] mipTexData = new byte[byteSize_estimation];

                //check to see if we are not over the header length (we are working backwards)
                if (leftoverOffset > headerLength)
                {
                    //copy all the bytes from the source byte file after the leftoverOffset, and copy that data to the texture data byte array
                    Array.Copy(sourceByteFile, leftoverOffset, mipTexData, 0, mipTexData.Length);

                    //combine the new mip byte data to the existing texture data byte array
                    textureData_reOrdered = byteUtils.CombineByteArray(textureData_reOrdered, mipTexData);
                }
            }

            //write the result to the console for viewing
            Console.WriteLine("Total Mips Byte Size = {0}", totalMipByteSize.ToString());

            //not required, but just viewing to see if our estimated sizes match up with the parsed texture byte size
            int totalTexByteSize = totalMipByteSize + mainTextureByteSize_Estimation;

            //write the result to the console for viewing
            Console.WriteLine("Total Byte Size = {0}", totalTexByteSize.ToString());

            //create our d3dtx object
            file_D3DTX = new File_D3DTX();

            //assign our values
            file_D3DTX.magic = parsed_dword;
            file_D3DTX.textureDataByteSize = parsed_textureDataByteSize;
            file_D3DTX.imageHeight = parsed_imageHeight;
            file_D3DTX.imageWidth = parsed_imageWidth;
            file_D3DTX.imageMipMapCount = parsed_imageMipMapCount;
            file_D3DTX.imageMipMapCount_decremented = parsed_imageMipMapCount_decremented;
            file_D3DTX.dxtType = parsed_dxtType;
            file_D3DTX.parsed_dxtType_string = parsed_dxtType_string;
            file_D3DTX.originalFileName = sourceFileName;
            file_D3DTX.fileNameInFileLength = parsed_fileNameInFileLength;
            file_D3DTX.fileNameInFile_string = parsed_fileNameInFile_string;
            file_D3DTX.headerLength = headerLength;
            file_D3DTX.filePath = sourceFile;
            file_D3DTX.mipMapByteSize = parsed_mipMapByteSize;

            //assign the main chunks of byte data
            file_D3DTX.headerData = headerData;
            file_D3DTX.textureData = textureData;
            file_D3DTX.textureData_reOrdered = textureData_reOrdered;

            //we are done!
            return file_D3DTX;
        }
    }
}
