<?php
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST');
header('Access-Control-Allow-Headers: Content-Type, Authorization');
$segments = explode('/', trim(strtok($_SERVER['REQUEST_URI'], '?'), '/'));
$uploadpath = __DIR__ . DIRECTORY_SEPARATOR . 'peerdata';
if (count($segments) <= 1) {
    $indexpath = $uploadpath . DIRECTORY_SEPARATOR . 'index.html';
    echo (file_exists($indexpath) ? file_get_contents($indexpath) : '<html><head></head><body><form action="/index.php/peer/upload" method="POST" enctype="multipart/form-data" target="_blank"><input type="file" name="file"><button type="submit">Save</button></form></body>');
    exit;
}
$addUrl = strpos($segments[0] ?? '', "php") ? ("/" . array_shift($segments)) : "";
if ($segments[0] === "peer" && $segments[1] === "upload" && $_SERVER['REQUEST_METHOD'] === 'POST') {
    if (!isset($_FILES['file']) || $_FILES['file']['error'] !== UPLOAD_ERR_OK) {
        exit;
    }
    $filename = basename($_FILES['file']['name']);
    if (in_array(preg_replace('/\d/', '', strtolower(pathinfo($filename, PATHINFO_EXTENSION))), ['phtml', 'inc', 'phps', 'php'])) {
        exit;
    }
    if (!is_dir($uploadpath)) {
        mkdir($uploadpath, 0777, true);
    }
    $i = 0;
    while (true) {
        $fileid = $i . '_' . $filename;
        $filepath = $uploadpath . DIRECTORY_SEPARATOR . $fileid;
        $i += 1;
        if (file_exists($filepath)) {
            continue;
        }
        if (move_uploaded_file($_FILES['file']['tmp_name'], $filepath)) {
            http_response_code(200);
            $protocol = (!empty($_SERVER['HTTPS']) && $_SERVER['HTTPS'] !== 'off' || $_SERVER['SERVER_PORT'] == 443) ? "https://" : "http://";
            echo $protocol . $_SERVER['HTTP_HOST'] . $addUrl . "/peer/" . $fileid;
            break;
        } else {
            if (file_exists($filepath)) {
                continue;
            }
            break;
        }
    }
} elseif ($segments[0] === "peer" && count($segments) <= 3 && $_SERVER['REQUEST_METHOD'] === 'GET') {
    $filepath = $uploadpath . DIRECTORY_SEPARATOR . urldecode($segments[1]);
    if (!file_exists($filepath)) {
        $filepath = $uploadpath . DIRECTORY_SEPARATOR . $segments[1];
    }
    if (file_exists($filepath)) {
        $contentTypes = [
            'jpeg'=>'image/jpeg', 'jpg'=>'image/jpeg', 'png'=>'image/png', 'gif'=>'image/gif', 'webp'=>'image/webp',
            'bmp'=>'image/bmp', 'svg'=>'image/svg+xml', 'ico'=>'image/x-icon',
            'mp4'=>'video/mp4', 'webm'=>'video/webm', 'mkv'=>'video/x-matroska',
            'mov'=>'video/quicktime', 'ogv'=>'video/ogg', 'flv'=>'video/x-flv',
            'mp3'=>'audio/mpeg','aac'=>'audio/aac','wav'=>'audio/wav', 'flac'=>'audio/flac',
            'ogg'=>'audio/ogg', 'opus'=>'audio/opus', 'm4a'=>'audio/mp4',
            'pdf'=>'application/pdf', 'zip'=>'application/zip', 'php'=>'application/x-php',
            'html'=>'text/html', 'css'=>'text/css', 'js'=>'application/javascript',
            'json'=>'application/json', 'xml'=>'application/xml', 'txt'=>'text/plain', 'csv'=>'text/csv',
            'woff'=>'font/woff', 'woff2'=>'font/woff2', 'rar'=>'application/vnd.rar', 'torrent'=>'application/x-bittorrent'
        ];
        $extension = strtolower(pathinfo($filepath, PATHINFO_EXTENSION));
        $contentType = isset($contentTypes[$extension]) ? $contentTypes[$extension] : 'application/octet-stream';
        header('Content-Type: ' . $contentType);
        echo file_get_contents($filepath);
    } else {
        http_response_code(404);
    }
} else {
    http_response_code(404);
}
?>