<?php
class File {
    function __construct(string $fileName, string $contentType, string $stream, int $length) {
        $this->fileName = $fileName;
        $this->contentType = $contentType;
        $this->stream = $stream;
        $this->length = $length;
    }

    public string $fileName;
    public string $contentType;
    public string $stream;
    public int $length;
}
?>