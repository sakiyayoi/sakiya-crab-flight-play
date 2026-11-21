#!/usr/bin/env node
'use strict';
// node harness/scripts/record_playthrough.js ＝ node harness/scripts/record.js playthrough と同じ
process.argv[2] = 'playthrough';
require('./record.js');
