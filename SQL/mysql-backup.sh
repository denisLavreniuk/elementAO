#!/bin/bash
 
################################################################
##
##   MySQL Database Backup Script 
##   Written By: Rahul Kumar
##   URL: https://tecadmin.net/bash-script-mysql-database-backup/
##   Last Update: Jan 05, 2019
##
################################################################
 
export PATH=/bin:/usr/bin:/usr/local/bin
TODAY=`date +"%d%b%Y"`
 
################################################################
################## Update below values  ########################
 
DB_BACKUP_PATH='/mnt/DB'
MYSQL_HOST='localhost'
MYSQL_PORT='3306'
MYSQL_USER='element_user'
MYSQL_PASSWORD='lohozavr'
DATABASE_NAME='element_db'
DATABASE2_NAME='element_templates'
BACKUP_RETAIN_DAYS=30   ## Number of days to keep local backup copy
 
#################################################################
if [ -d ${DB_BACKUP_PATH} ]; then
	echo "Device /dev/sda1 allready mounted."
else
	echo "Trying to mount /dev/sda1 to /mnt dir"
	mount /dev/sda1 /mnt
	if [ -d ${DB_BACKUP_PATH} ] ; then 
		echo "Device /dev/sda1 was succesfull mounted to /mnt"
	else
		echo "Can't mount /dev/sda1 to /mnt"
		exit 1
	fi
fi
 
mkdir -p ${DB_BACKUP_PATH}/${TODAY}
echo "Backup started for database - ${DATABASE_NAME}"
 
 
mysqldump -h ${MYSQL_HOST} \
   -P ${MYSQL_PORT} \
   -u ${MYSQL_USER} \
   -p${MYSQL_PASSWORD}\
   -B ${DATABASE_NAME} ${DATABASE2_NAME} | gzip > ${DB_BACKUP_PATH}/${TODAY}/${DATABASE_NAME}-${TODAY}.sql.gz

if [ $? -eq 0 ]; then
  logger "Database backup successfully completed"
else
  logger "Error found during backup"
  exit 1
fi
 
 
##### Remove backups older than {BACKUP_RETAIN_DAYS} days  #####
 
DBDELDATE=`date +"%d%b%Y" --date="${BACKUP_RETAIN_DAYS} days ago"`
 
if [ ! -z ${DB_BACKUP_PATH} ]; then
      cd ${DB_BACKUP_PATH}
      if [ ! -z ${DBDELDATE} ] && [ -d ${DBDELDATE} ]; then
            rm -rf ${DBDELDATE}
      fi
fi

cd

if [  -d ${DB_BACKUP_PATH} ]; then
	umount /dev/sda1
	if [ ! -d ${DB_BACKUP_PATH} ]; then
		echo "/mnt dir was succesfully UNMOUNTED"
	else
		echo "Can't unmount /mnt dir"
	fi
fi 
### End of script ####
