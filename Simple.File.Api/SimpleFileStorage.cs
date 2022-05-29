using System.Security.Cryptography;
using Simple.File.Api.Interfaces;

namespace Simple.File.Api;


public class SimpleFileStorage : ISimpleFileStorage
{
    public bool IsFileLocked(string filepath)
    {
        try
        {
            using (var stream = System.IO.File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                stream.Close();
            }
        }
        catch (IOException e) 
        {
            //  The file is missing, or the file or directory is in use.
            return true;
        }


        return false;
    }
    public async Task Store(Stream streamToStore, string filepath)
    {
        using (var targetFilestream = System.IO.File.Create(filepath))
        {
            await streamToStore.CopyToAsync(targetFilestream);
        }
    }

    public void Delete(string filepath)
    {
        // Not sure if this "isLocked" trick is enough for production use:
        // while we check and get a return value and proceed to delete another thread could
        // start a sha256 calculation again. Maybe Mutex instead.
        if (System.IO.File.Exists(filepath) && !IsFileLocked(filepath))
        {
            System.IO.File.Delete(filepath);
        }
    }

    public string CalculateSha256(string filepath)
    {
        if (!System.IO.File.Exists(filepath))
            return string.Empty;
        
        using (var sha256 = SHA256.Create())
        {
            using (var fileStream = System.IO.File.OpenRead(filepath))
            {
                var hash = sha256.ComputeHash(fileStream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}