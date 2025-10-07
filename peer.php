<?php
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST');
header('Access-Control-Allow-Headers: Content-Type, Authorization');
$uploadFolder = __DIR__ . DIRECTORY_SEPARATOR . 'peer';
$segments = explode('/', trim(strtok($_SERVER['REQUEST_URI'], '?'), '/'));
$addUrl = strpos($segments[0] ?? '', "php") ? ("/" . array_shift($segments)) : "";
if (count($segments) !== 2 || $segments[0] !== "peer") {
    http_response_code(404);
    exit;
}
if ($segments[1] === "upload" && $_SERVER['REQUEST_METHOD'] === 'POST') {
    if (!isset($_FILES['file']) || $_FILES['file']['error'] !== UPLOAD_ERR_OK) {
        http_response_code(400);
        exit;
    }
    if (!is_dir($uploadFolder)) {
        mkdir($uploadFolder, 0777, true);
    }
    $filename = basename($_FILES['file']['name']);
    $extension = strtolower(pathinfo($filename, PATHINFO_EXTENSION));
    if (preg_replace('/\d/', '', $extension) === 'php') {
        $normalizedContent = preg_replace('/\s+/', '', file_get_contents($_FILES['file']['tmp_name']));
        if (strpos($normalizedContent, '<?') !== false && strpos($normalizedContent, '?>') !== false) {
            exit;
        }
    }
    if (in_array($extension, ['phtml', 'inc', 'phps'])) {
        exit;
    }
    $i = 0;
    while (true) {
        $fileid = $i . '_' . $filename;
        $filepath = $uploadFolder . DIRECTORY_SEPARATOR . $fileid;
        $i += 1;
        if (file_exists($filepath)) {
            continue;
        }
        if (move_uploaded_file($_FILES['file']['tmp_name'], $filepath)) {
            $protocol = (!empty($_SERVER['HTTPS']) && $_SERVER['HTTPS'] !== 'off' || $_SERVER['SERVER_PORT'] == 443) ? "https://" : "http://";
            http_response_code(200);
            echo $protocol . $_SERVER['HTTP_HOST'] . $addUrl . "/peer/" . $fileid;
            break;
        }
        if (!file_exists($filepath)) {
            http_response_code(500);
            break;
        }
    }
} elseif ($segments[1] === "index") {
    echo '<!DOCTYPE html><html><head><style>body { background:#121212; } input, a { font-size:40px; display:block; color:#fff; }
          </style></head><body><input type="file" id="in" onchange="upload();" />
          <script>async function upload() {
            const fd = new FormData(); fd.append("file", document.getElementById("in").files[0]);
            const url = (await (await fetch(location.origin + "/peer.php/peer/upload", { method: "POST", body: fd })).text()).replace("/peer.php/", "/");
            document.body.innerHTML += \'<a target="_blank" href="\' + url + \'">\' + url + "</a>";
          }</script></body></html>';
} elseif ($_SERVER['REQUEST_METHOD'] === 'GET') {
    $filepath = $uploadFolder . DIRECTORY_SEPARATOR . urldecode($segments[1]);
    if (file_exists($filepath)) {
        if (function_exists('mime_content_type')) {
            header('Content-Type: ' . mime_content_type(urldecode($segments[1])));
        } else {
            $mimeTypes = ['txt'=>'text/plain', 'json'=>'application/json', 'xml'=>'application/xml', 'pdf'=>'application/pdf', 'zip'=>'application/zip', 'html'=>'text/html', 'php'=>'application/x-php',
                          'jpg'=>'image/jpeg', 'jpeg'=>'image/jpeg', 'png'=>'image/png', 'gif'=>'image/gif', 'svg'=>'image/svg+xml', 'mp4'=>'video/mp4', 'mp3'=>'audio/mpeg', 'wav'=>'audio/wav'];
            $extension = strtolower(pathinfo($filepath, PATHINFO_EXTENSION));
            header('Content-Type: ' . (isset($mimeTypes[$extension]) ? $mimeTypes[$extension] : "application/octet-stream"));
        }
        echo file_get_contents($filepath);
    } else {
        http_response_code(404);
    }
}
?>
