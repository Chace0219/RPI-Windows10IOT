<!doctype html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="content-type" content="text/html; charset=utf-8" />
<title>ForkLift Management System</title>


<script type="text/javascript" charset="utf-8" src="<?php echo $_path_js?>jquery.min.js"></script>
<script type="text/javascript" charset="utf-8" src="<?php echo $_path_js?>grid.locale-en.js"></script>
<script type="text/javascript" charset="utf-8" src="<?php echo $_path_js?>jquery.jqGrid.min.js"></script>

<link href="<?php echo $_path_css?>default.css" rel="stylesheet" type="text/css" />
<!--<link rel="stylesheet" href="<?php echo $_path_css?>bootstrap.min.css">-->
<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css">
<link href="<?php echo $_path_css?>ui.jqgrid-bootstrap.css" rel="stylesheet" type="text/css" />

<link href="<?php echo $_path_css?>ui.jqgrid-bootstrap.css" rel="stylesheet" type="text/css" />
<link href="<?php echo $_path_css?>bootstrap-datepicker.css" rel="stylesheet" type="text/css" />


<script type="text/javascript" charset="utf-8" src="<?php echo $_path_js?>jquery-ui.min.js"></script>
<script type="text/javascript" charset="utf-8" src="<?php echo $_path_js?>bootstrap-datepicker.js"></script>
<link rel="shortcut icon" type="image/ico" href="favicon.ico">

</head>
<body>
<!-- start header -->

<?php
	$first_selected = ($file == "index")?"active":"";
	$second_selected = ($file == "second")?"active":"";
?>
<div id="header">
	<div id="logo">
		<h1><a href="#">ForkLift Management<sup>1.0</sup></a></h1>
		<h2>By Rob Niessen</h2>
	</div>
	<div id="menu">
		<ul>
			<li class="<?php echo $first_selected;?>"><a href="?"> All Event</a></li>
			<li class="<?php echo $second_selected;?>"><a href="?b_m=bond&file=second">Working Hours</a></li>
		</ul>
	</div>
</div>
<div style="height:70px;"></div>
<!-- end header -->