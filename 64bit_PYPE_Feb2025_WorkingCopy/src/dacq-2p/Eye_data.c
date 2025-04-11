#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/time.h>

#include "Eye_data.h"


/*----------------------------------------------------------------------
Point related functions
------------------------------------------------------------------------*/
//Point contains: float x;float y;
void Point_set_x(Point* point,double value){
    point->x = value;
}
void Point_set_y(Point* point,double value){
    point->y = value;
}
Point* Point_sub(Point* point1,Point* point2){
    Point* point3 = malloc(sizeof(Point));
    if (point3 != NULL){
        point3->x = point1->x - point2->x;
        point3->y = point1->y - point2->y;
        return point3;
    }else{
        printf("Warning: new point not created due to memory allocation failed. \n");
        return NULL;
    }
}
Point* Point_add(Point* point1,Point* point2){
    Point* point3 = malloc(sizeof(Point));
    if (point3 != NULL){
        point3->x = point1->x + point2->x;
        point3->y = point1->y + point2->y;
        return point3;
    }else{
        printf("Warning: new point not created due to memory allocation failed. \n");
        return NULL;
    }
}


/*-------------------------------------------------------
Eyedata-related function.
----------------------------------------------------------*/
static void EyeData_init(EyeData* data,json_object* json){
    json_object* temp = json_object_object_get(json,"FrameNumber");
    data->frame_number = json_object_get_int(temp);
    temp = json_object_object_get(json,"Pupil");
    temp = json_object_object_get(temp,"Center");
    temp = json_object_object_get(temp,"X");
    Point_set_x(&(data->pupil),json_object_get_double(temp));
    temp = json_object_object_get(json,"Pupil");
    temp = json_object_object_get(temp,"Center");
    temp = json_object_object_get(temp,"Y");
    Point_set_y(&(data->pupil),json_object_get_double(temp));

    temp = json_object_object_get(json,"Pupil");
    temp = json_object_object_get(temp,"Size");
    temp = json_object_object_get(temp,"Width");
    data->pupil_area = json_object_get_double(temp);
    temp = json_object_object_get(json,"Pupil");
    temp = json_object_object_get(temp,"Size");
    temp = json_object_object_get(temp,"Height");
    data->pupil_area *= json_object_get_double(temp);

    temp = json_object_object_get(json,"CRs");
    if (temp!=NULL){
        temp = json_object_array_get_idx(temp,0);
        temp = json_object_object_get(temp,"X");
        Point_set_x(&(data->cr),json_object_get_double(temp));
		//fprintf(stderr,"Get an x position: %.2f\n",json_object_get_double(temp));
        temp = json_object_object_get(json,"CRs");
        temp = json_object_array_get_idx(temp,0);
        temp = json_object_object_get(temp,"Y");
        Point_set_y(&(data->cr),json_object_get_double(temp));
		//fprintf(stderr,"Get an y position: %.2f\n",json_object_get_double(temp));
    }else{
        printf("No cr position received from OpenIris. \n");
    }
    temp = json_object_object_get(json,"CRs");

    temp = json_object_array_get_idx(temp,3);
    temp = json_object_object_get(temp,"X");
    Point_set_x(&(data->p4),json_object_get_double(temp));
    temp = json_object_object_get(json,"CRs");
    temp = json_object_array_get_idx(temp,3);
    temp = json_object_object_get(temp,"Y");
    Point_set_y(&(data->p4),json_object_get_double(temp));
}

/*-------------------------------------------------------
Eyesdata-related function.
----------------------------------------------------------*/
void EyesData_init(json_object* json,EyesData* eyesdata){
    json_object* temp = json_object_object_get(json,"Left");
    EyeData_init(&(eyesdata->left),temp);
	//printf("Openiris_server: Finish get left eye data \n");
    temp = json_object_object_get(json,"Right");
    EyeData_init(&(eyesdata->right),temp);
	//printf("Openiris_server: Finish get right eye data \n");
}
