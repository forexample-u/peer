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
} elseif ($_SERVER['REQUEST_METHOD'] === 'GET') {
    $filepath = $uploadFolder . DIRECTORY_SEPARATOR . urldecode($segments[1]);
    if (file_exists($filepath)) {
        header('Content-Type: ' . mime_content_type(urldecode($segments[1])));
        echo file_get_contents($filepath);
    } else {
        http_response_code(404);
    }
}
?>