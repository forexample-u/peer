<?php
require_once __DIR__ . '/file.php';

class Message {
    function __construct(string $id, string $text, File $file) {
        $this->id = $id;
        $this->text = $text;
        $this->file = $file;
    }

    public string $id;
    public string $text;
    public File $file;
}
?>