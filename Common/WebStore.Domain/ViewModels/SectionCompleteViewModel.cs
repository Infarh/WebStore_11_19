﻿using System;
using System.Collections.Generic;
using System.Text;

namespace WebStore.Domain.ViewModels
{
    public class SectionCompleteViewModel
    {
        public IEnumerable<SectionViewModel> Sections { get; set; }

        public int? CurrentParrentSection { get; set; }

        public int? CurrentSectionId { get; set; }
    }
}
