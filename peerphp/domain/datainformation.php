<?php
class DataInformation {
    public function __construct($countQuery, $querySizeMB) {
        $this->countQuery = $countQuery;
        $this->querySizeMB = $querySizeMB;
    }
    
    public int $id = 1;
    public int $countQuery;
    public float $querySizeMB;
}
?>