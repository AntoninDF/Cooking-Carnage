\# To Do



Les EventBindings n'ont rien à faire ici. DynamicCodebase est censé être un système standalone.
Les EventBindings permettent de marquer certains events comme globaux, pour y accéder via l'event linker. L'event linker devrait être supprimé, et les events globaux ne devraient passer que par l'event display (qui fait pareil mais avec des events locaux). En bref : rassembler les events locaux et globaux.

