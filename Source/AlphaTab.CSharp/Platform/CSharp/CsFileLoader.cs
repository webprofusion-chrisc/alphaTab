/*
 * This file is part of alphaTab.
 * Copyright (c) 2014, Daniel Kuschny and Contributors, All rights reserved.
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3.0 of the License, or at your option any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library.
 */

using System;
using System.IO;
using System.Threading.Tasks;

namespace AlphaTab.Platform.CSharp
{
    /// <summary>
    /// This file loader loads binary files using the native apis
    /// </summary>
    public class CsFileLoader : IFileLoader
    {
#if WINDOWS_UWP

        public byte[] LoadBinary(string path)
        {
            var result = Task.Run(async () =>
            {
                var folder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile file = await folder.GetFileAsync("Fade To Black.gp4");

                byte[] fileBytes = null;
                using (var stream = await file.OpenReadAsync())
                {
                    fileBytes = new byte[stream.Size];
                    using (var reader = new Windows.Storage.Streams.DataReader(stream))
                    {
                        await reader.LoadAsync((uint)stream.Size);
                        reader.ReadBytes(fileBytes);
                    }
                }
                return fileBytes;
            }
            );

            result.Wait();
            return result.Result;
        }

#else
         public byte[] LoadBinary(string path)
        {
            return File.ReadAllBytes(path);
        }
#endif

        public void LoadBinaryAsync(string path, Action<byte[]> success, Action<Exception> error)
        {
            try
            {
                success(LoadBinary(path));
            }
            catch (Exception e)
            {
                error(e);
            }
        }
    }
}