#include <stdio.h>
#include <unistd.h>

int main(int ac, char **av)
{
  execvp(av[1], av+1);
}

/*
int main(int ac, char **av)
{
  char **newav;
  int newac, n;

  newac = ac + 1;
  newav = (char **) calloc(sizeof(char *), (newac + 10));

  newav[0] = PYTHONEXE;
  for (n = 1; n < (ac+1); n++) {
    newav[0+n] = av[n];
  }
  if (geteuid() != 0) {
    fprintf(stderr, "pypeboot: warning not suid root.\n");
  }
  execvp(newav[0], newav+1);
}
*/
