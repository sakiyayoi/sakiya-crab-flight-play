#!/usr/bin/env node
'use strict';
// node harness/scripts/record_compare.js ＝ node harness/scripts/record.js compare と同じ
process.argv[2] = 'compare';
require('./record.js');
