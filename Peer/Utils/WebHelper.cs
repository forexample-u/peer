namespace Peer.Utils;

public static class WebHelper
{
    public static string GetContentTypeByExtension(string extension)
    {
        string contentType = "";
        switch (extension.TrimStart('.').ToLower())
        {
            case "jpg": contentType = "image/jpeg"; break;
            case "jpeg": contentType = "image/jpeg"; break;
            case "png": contentType = "image/png"; break;
            case "gif": contentType = "image/gif"; break;
            case "bmp": contentType = "image/bmp"; break;
            case "pdf": contentType = "application/pdf"; break;
            case "doc": contentType = "application/msword"; break;
            case "docx": contentType = "application/msword"; break;
            case "xls": contentType = "application/vnd.ms-excel"; break;
            case "xlsx": contentType = "application/vnd.ms-excel"; break;
            case "ppt": contentType = "application/vnd.ms-powerpoint"; break;
            case "pptx": contentType = "application/vnd.ms-powerpoint"; break;
            case "eot": contentType = "application/vnd.ms-fontobject"; break;
            case "rar": contentType = "application/x-rar-compressed"; break;
            case "7z": contentType = "application/x-7z-compressed"; break;
            case "json": contentType = "application/json"; break;
            case "zip": contentType = "application/zip"; break;
            case "avi": contentType = "video/x-msvideo"; break;
            case "mov": contentType = "video/quicktime"; break;
            case "mp3": contentType = "audio/mpeg"; break;
            case "mp4": contentType = "video/mp4"; break;
            case "txt": contentType = "text/plain"; break;
            case "csv": contentType = "text/csv"; break;
            case "html": contentType = "text/html"; break;
            case "xml": contentType = "application/xml"; break;
            case "tar": contentType = "application/x-tar"; break;
            case "svg": contentType = "image/svg+xml"; break;
            case "woff": contentType = "font/woff"; break;
            case "woff2": contentType = "font/woff2"; break;
            case "scss": contentType = "text/x-scss"; break;
            case "ttf": contentType = "font/ttf"; break;
            case "less": contentType = "text/css"; break;
            case "md": contentType = "text/markdown"; break;
            case "sql": contentType = "application/sql"; break;
            case "bat": contentType = "application/bat"; break;
            default: contentType = "application/octet-stream"; break;
        }
        return contentType;
    }
}