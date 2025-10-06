using Microsoft.AspNetCore.Mvc;

namespace Peer.Controllers;

public class Message
{
    [FromForm(Name = "file")]
    public IFormFile? File { get; set; }
}

[ApiController]
[Route("peer")]
public class PeerController : ControllerBase
{
    [HttpPost("upload")]
    public string Upload(Message message)
    {
        if (message.File == null || message.File.Length > 28311552)
        {
            return "";
        }
        Directory.CreateDirectory(Path.Combine("wwwroot", "peer"));
        string filename = message.File.FileName ?? "";
        for (int i = 0; ; i++)
        {
            string id = i.ToString() + "_" + filename;
            if (System.IO.File.Exists(Path.Combine("wwwroot", "peer", id)))
            {
                continue;
            }
            try
            {
                using var stream = new FileStream(Path.Combine("wwwroot", "peer", id), FileMode.CreateNew);
                message.File.CopyTo(stream);
                return $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/peer/{id}";
            }
            catch
            {
                if (System.IO.File.Exists(Path.Combine("wwwroot", "peer", id)))
                {
                    continue;
                }
                return "";
            }
        }
    }

    [HttpGet("index")]
    public IActionResult Index()
    {
        return Content(@"<!DOCTYPE html><html><head><style>body{background:#121212;}input,a{font-size:40px;display:block;color:#fff;}</style></head><body>
            <input type=""file"" id=""in"" onchange=""upload();""><script>JSON.parse(localStorage.getItem(""files"") || ""[]"").forEach(url => document.body.innerHTML += '<a target=""_blank"" href=""'+url+'"">'+url+""</a>"");
            async function upload() { 
              const fd = new FormData(); fd.append(""file"", document.getElementById(""in"").files[0]);
              const url = await (await fetch(location.origin+""/peer/upload"", {method:""POST"",body:fd})).text();
              document.body.innerHTML += '<a target=""_blank"" href=""'+url+'"">'+url+""</a>"";
              localStorage.setItem(""files"", JSON.stringify([...JSON.parse(localStorage.getItem(""files"") || ""[]""), url]));
            }</script></body></html>", "text/html");
    }
}