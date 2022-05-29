using Microsoft.Net.Http.Headers;

namespace Simple.File.Api.Utilities;

public static class FileHelper
{
    public static bool HasFileContentDisposition(ContentDispositionHeaderValue? contentDisposition)
    {
        return contentDisposition != null
               && contentDisposition.DispositionType.Equals("form-data")
               && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                   || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));

    }
}