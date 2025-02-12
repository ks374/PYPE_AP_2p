#ifndef EYE_DATA_H
#define EYE_DATA_H

#include <json-c/json.h>

typedef struct {
    double x;
    double y;
} Point;

typedef struct {
    int frame_number;
    Point pupil;
    double pupil_area;
    Point cr;
    Point p4;
} EyeData;

typedef struct {
    EyeData left;
    EyeData right;
} EyesData;

void Point_set_x(Point* point,double value);
void Point_set_y(Point* point,double value);
Point* Point_sub(Point* point1,Point* point2);
Point* Point_add(Point* point1,Point* point2);
void Eyesdata_update(json_object* json,EyesData* eyesdata);


#endif