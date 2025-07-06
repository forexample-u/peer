<?php
require_once __DIR__ . '/domain/data.php';
require_once __DIR__ . '/domain/datainformation.php';
require_once __DIR__ . '/domain/file.php';
require_once __DIR__ . '/domain/message.php';
require_once __DIR__ . '/application/minidb.php';
require_once __DIR__ . '/config.php';

$config = new Config();
if (!isset($GLOBALS['db'])) {
    $config::load();
    $GLOBALS['db'] = new Minidb();
}

$db = $GLOBALS['db'];
$method = $_SERVER['REQUEST_METHOD'];
$segments = explode('/', trim(substr(strtok($_SERVER['REQUEST_URI'], '?'), strlen('/peer')), '/'));
header('Content-Type: application/json; charset=utf-8');
switch ($segments[0]) {
    case 'write':
        if ($method === 'POST') {
            $datainformation = $db->get_info();
            if ($datainformation->countQuery % 20 == 0) {
                $db->shrink();
            }
            $text = $_POST['text'] ?? "";
            $file = new File("", "", "", 0);
            if (isset($_FILES['file']) && $_FILES['file']['error'] === UPLOAD_ERR_OK) {
                $stream = file_get_contents($_FILES['file']['tmp_name']);
                $file = new File($_FILES['file']['name'], $_FILES['file']['type'], $stream, $_FILES['file']['size']);
            }
            $message = new Message($segments[1], $text, $file);
            $deleteUnixAt = $db->write($message, $datainformation->querySizeMB);
            echo $deleteUnixAt;
        } elseif ($method === 'GET') {
            $datainformation = $db->get_info();
            if ($datainformation->countQuery % 20 == 0) {
                $db->shrink();
            }
            $datainformation = $db->get_info();
            $message = new Message($segments[1], $segments[2], new File("", "", "", 0));
            $deleteUnixAt = $db->write($message, $datainformation->querySizeMB);
            echo $deleteUnixAt;
        } else {
            http_response_code(405);
        }
        break;

    case 'text':
        $text = $db->get_text($segments[1]);
        echo $text;
        break;

    case 'file':
        $data = $db->get_file($segments[1]);
        if ($data->filename !== "" && $data->filelength !== 0) {
            header('Content-Type: ' . $data->contentType);
            header("Content-Disposition: inline; filename*=UTF-8''" . rawurlencode($data->filename));
            header('Content-Length: ' . $data->filelength);
            echo $data->bytes;
        } else {
            http_response_code(404);
        }
        exit;

    case 'download':
        $data = $db->get_file($segments[1]);
        if ($data->filename !== "" && $data->filelength !== 0) {
            header('Content-Type: ' . $data->contentType);
            header("Content-Disposition: attachment; filename*=UTF-8''" . rawurlencode($data->filename));
            header('Content-Length: ' . $data->filelength);
            echo $data->bytes;
        } else {
            http_response_code(404);
        }
        break;

    default:
        http_response_code(404);
        break;
}
?>