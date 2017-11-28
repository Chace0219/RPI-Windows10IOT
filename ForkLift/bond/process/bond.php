<?php
	include("../../src/config.php");
	$cmd = isset($_GET['cmd'])?$_GET['cmd']:"";

	$o_bond = new Bond();
	switch($cmd)
	{		
		case "getTurn":		
			echo $o_bond->getTurn();
			break;
		default:
			echo $o_bond->getRecords();
			break;
	}
?>