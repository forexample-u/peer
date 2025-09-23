<?php
$uploadFolder = __DIR__ . '/peer';
$segments = explode('/', trim(substr(strtok($_SERVER['REQUEST_URI'], '?'), strlen('/peer')), '/'));
if (strpos($segments[0], "php") !== false) {
    array_shift($segments);
}
if ($segments[0] === "peer") {
    array_shift($segments);
}
header('Content-Type: application/json; charset=utf-8');
header("Access-Control-Allow-Origin: *");
switch ($segments[0]) {
    case 'upload':
        if ($_SERVER['REQUEST_METHOD'] === 'POST') {
            if (!isset($_FILES['file']) || $_FILES['file']['error'] !== UPLOAD_ERR_OK) {
                http_response_code(400);
                echo "";
                break;
            }
            if (!is_dir($uploadFolder)) {
                mkdir($uploadFolder, 0777, true);
            }
            $filename = basename($_FILES['file']['name']);
            $i = 0;
            while (true) {
                $fileid = $i . '_' . $filename;
                $i += 1;
                $filepath = $uploadFolder . DIRECTORY_SEPARATOR . $fileid;
                if (file_exists($filepath)) {
                    continue;
                }
                if (move_uploaded_file($_FILES['file']['tmp_name'], $filepath)) {
                    $protocol = (!empty($_SERVER['HTTPS']) && $_SERVER['HTTPS'] !== 'off' || $_SERVER['SERVER_PORT'] == 443) ? "https://" : "http://";
                    $url = $protocol . $_SERVER['HTTP_HOST'] . "/peer/" . $fileid;
                    http_response_code(200);
                    echo $url;
                    break;
                } else {
                    if (file_exists($filepath)) {
                        continue;
                    }
                    http_response_code(500);
                    echo "";
                    break;
                }
            }
        } else {
            http_response_code(405);
        }
        break;

    default:
        $filepath = $uploadFolder . "/" . urldecode($segments[0]);
        if (file_exists($filepath)) {
            header('Content-Type: ' . mime_content_type(urldecode($segments[0])));
            echo file_get_contents($filepath);
        } else {
            http_response_code(404);
        }
        break;
}
?>