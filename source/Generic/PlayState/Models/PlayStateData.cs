﻿using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayState.Models
{
    public class PlayStateData
    {
        public Game Game { get; set; }
        public DateTime StartDate { get; set; }
        public Stopwatch Stopwatch { get; set; }

        public PlayStateData(Game game)
        {
            Game = game;
            StartDate = DateTime.Now;
            Stopwatch = new Stopwatch();
        }
    }
}