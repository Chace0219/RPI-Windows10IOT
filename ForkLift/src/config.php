<?php
	define('FPATH_BASE', $_SERVER["DOCUMENT_ROOT"]."/ForkLift");
	define('APATH_BASE', $_SERVER["SERVER_NAME"]."/");

	define('SERVER_HOST',"http://".$_SERVER["SERVER_NAME"]);	
	define('INCLUDE_PATH',  FPATH_BASE . "include/" );

	function __autoload($class_name) //if tmp is not used del acsrview
    {
		$dir_array = array("bond");
		foreach($dir_array as $dir)
		{
			if(file_exists(FPATH_BASE.'/src/classes/control/'.$dir.'/'.$class_name.'.php'))
				require_once FPATH_BASE.'/src/classes/control/'.$dir.'/'.$class_name.'.php';
			else if(file_exists(FPATH_BASE.'/src/classes/model/process/'.$dir.'/'.$class_name.'.php'))
				require_once FPATH_BASE.'/src/classes/model/process/'.$dir.'/'.$class_name.'.php';
			else if(file_exists(FPATH_BASE.'/src/classes/control/'.$class_name.'.php'))
				require_once FPATH_BASE.'/src/classes/control/'.$class_name.'.php';
			else if(file_exists(FPATH_BASE.'/src/classes/model/process/'.$class_name.'.php'))
				require_once FPATH_BASE.'/src/classes/model/process/'.$class_name.'.php';
		}
        clearstatcache();
    }

	$_404_page = str_replace($_SERVER["DOCUMENT_ROOT"],'', FPATH_BASE).'/404.php';	

	/* contents path */
	$_path = 'contents/';
	$_path_img = $_path.'images/';
	$_path_css = $_path.'css/';
	$_path_js = $_path.'js/';
	/********/
	
	require FPATH_BASE.'/src/constant.php';
	require FPATH_BASE.'/src/reference/library.php';

	define('DBHOST', 'localhost');
	define('DBNAME', 'forklift');
	define('DBUSER', 'root');
	define('DBPASSWD', '');

	define('TBL_REPORT_LIST', 'report_list');
	define('TBL_RPI_LIST', 'rpi_list');
	define('TBL_SETTING_LIST', 'setting_list');
	define('TBL_USER_INFO', 'user_info');

	$_db = mysqli_connect(DBHOST, DBUSER, DBPASSWD) or die('Fail DB Server Connection.');
	mysqli_select_db($_db, DBNAME);
	
?>