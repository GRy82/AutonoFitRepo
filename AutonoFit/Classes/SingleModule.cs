using AutonoFit.Classes;
using AutonoFit.Contracts;
using AutonoFit.Services;
using AutonoFit.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Classes
{
    public class SingleModule
    {
        private ExerciseLibraryService _exerciseLibraryService;

        public SingleModule(ExerciseLibraryService exerciseLibraryService)
        {
            _exerciseLibraryService = exerciseLibraryService;
        }


       
    }
}
