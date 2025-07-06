<?php
class Data {
    function __construct(string $uniqueKey, string $text, int $filehash, string $filename, int $filelength, string $deleteUnixAt, string $contentType, float $querySizeMB, $bytes) {
        $this->uniqueKey = $uniqueKey;
        $this->text = $text;
        $this->filehash = $filehash;
        $this->filename = $filename;
        $this->filelength = $filelength;
        $this->deleteUnixAt = $deleteUnixAt;
        $this->contentType = $contentType;
        $this->querySizeMB = $querySizeMB;
        $this->bytes = $bytes;
    }

    public string $uniqueKey;
    public string $text;
    public int $filehash;
    public string $filename;
    public int $filelength;
    public string $deleteUnixAt;
    public string $contentType;
    public float $querySizeMB;
    public $bytes;
}
?>