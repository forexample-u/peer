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
if (strpos($segments[0], "php") !== false) {
    array_shift($segments);
}
if ($segments[0] === "peer") {
    array_shift($segments);
}
header('Content-Type: application/json; charset=utf-8');
header("Access-Control-Allow-Origin: *");
switch ($segments[0]) {
    case 'write':
        if ($method === 'POST') {
            $mysqli = new mysqli($config::$MysqlHost , $config::$MysqlUser, $config::$MysqlPassword, $config::$MysqlDatabase, $config::$MysqlPort);
            $datainformation = $db->get_info($mysqli);
            if ($datainformation->countQuery % 20 == 0) {
                $db->shrink($mysqli);
            }
            $text = $_POST['text'] ?? "";
            $file = new File("", "", "", 0);
            if (isset($_FILES['file']) && $_FILES['file']['error'] === UPLOAD_ERR_OK) {
                $stream = file_get_contents($_FILES['file']['tmp_name']);
                $file = new File($_FILES['file']['name'], $_FILES['file']['type'], $stream, $_FILES['file']['size']);
            }
            $message = new Message($segments[1], $text, $file);
            $deleteUnixAt = $db->write($mysqli, $message, $datainformation->querySizeMB);
            $mysqli->close();
            echo $deleteUnixAt;
        } elseif ($method === 'GET') {
            $mysqli = new mysqli($config::$MysqlHost , $config::$MysqlUser, $config::$MysqlPassword, $config::$MysqlDatabase, $config::$MysqlPort);
            $datainformation = $db->get_info($mysqli);
            if ($datainformation->countQuery % 20 == 0) {
                $db->shrink($mysqli);
            }
            $message = new Message($segments[1], $segments[2], new File("", "", "", 0));
            $deleteUnixAt = $db->write($mysqli, $message, $datainformation->querySizeMB);
            $mysqli->close();
            echo $deleteUnixAt;
        } else {
            http_response_code(405);
        }
        break;

    case 'text':
        $mysqli = new mysqli($config::$MysqlHost , $config::$MysqlUser, $config::$MysqlPassword, $config::$MysqlDatabase, $config::$MysqlPort);
        $text = $db->get_text($mysqli, $segments[1]);
        $mysqli->close();
        echo $text;
        break;

    case 'file':
        $mysqli = new mysqli($config::$MysqlHost , $config::$MysqlUser, $config::$MysqlPassword, $config::$MysqlDatabase, $config::$MysqlPort);
        $data = $db->get_file($mysqli, $segments[1]);
        $mysqli->close();
        if ($data->filename !== "" && $data->filelength !== 0) {
            header('Content-Type: ' . $data->contentType);
            header("Content-Disposition: inline; filename*=UTF-8''" . rawurlencode($data->filename));
            header('Content-Length: ' . $data->filelength);
            header('Cache-Control: public, max-age=86400');
            echo $data->bytes;
        } else {
            http_response_code(404);
        }
        exit;

    case 'download':
        $mysqli = new mysqli($config::$MysqlHost , $config::$MysqlUser, $config::$MysqlPassword, $config::$MysqlDatabase, $config::$MysqlPort);
        $data = $db->get_file($mysqli, $segments[1]);
        $mysqli->close();
        if ($data->filename !== "" && $data->filelength !== 0) {
            header('Content-Type: ' . $data->contentType);
            header("Content-Disposition: attachment; filename*=UTF-8''" . rawurlencode($data->filename));
            header('Content-Length: ' . $data->filelength);
            header('Cache-Control: public, max-age=86400');
            echo $data->bytes;
        } else {
            http_response_code(404);
        }
        break;

    case $config::$SecretUrlInitDatabase:
        $mysqli = new mysqli($config::$MysqlHost , $config::$MysqlUser, $config::$MysqlPassword, $config::$MysqlDatabase, $config::$MysqlPort);
        $db->init($mysqli);
        $mysqli->close();
        echo "peer";
        break;

    default:
        http_response_code(404);
        break;
}
?>