<?php
	define('DBHOST', 'localhost');
	define('DBNAME', 'forklift');
	define('DBUSER', 'root');
	define('DBPASSWD', '');

	define('TBL_USER_INFO', 'user_info');
	define('TBL_RPI_LIST', 'rpi_list');
	define('TBL_SETTING_LIST', 'setting_list');
	define('TBL_REPORT_LIST', 'report_list');

	function exeQuery($sql){	
		global $_db;
		mysqli_query($_db, "set names utf8");
		$result = mysqli_query($_db, $sql);
		return $result;
	}
	function getResult($sql){
	
		$result = exeQuery($sql);
		$array = array();

		if(!$result) {return array();}

		if(mysqli_num_rows($result)==0){
			return array();
		}
		else{
			$nameArray=array();
			$j=0;				
			while ($j < mysqli_num_fields($result)) {
				$nameArray[$j] = mysqli_fetch_field_direct($result, $j);					
				$j++;
			}

			$i=0;	
			while($row= mysqli_fetch_array($result)){
				$array[$i] = array();

				foreach($nameArray as $name_obj){
					$name = $name_obj->name;					
					$array[$i][$name] = $row[$name];
				
				}
				$i++;
			}
			return $array;
		}	
		
		return $array;	
	}

	$_db = mysqli_connect(DBHOST, DBUSER, DBPASSWD) or die('Fail DB Server Connection.');
	mysqli_select_db($_db, DBNAME);
	$result_array = array();


	$query = isset($_POST["query"])?$_POST["query"]:"";
	//$query = "select * from user_info";
//echo $query;
	if($query != "")
	{
		$result_array = getResult($query);
	}
	print_r(json_encode($result_array));

	
?>