// Copyright 2025 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// This file defines the 'tail-logs' (alias 'logs') command for the clasp CLI.
import chalk from 'chalk';
import { Command } from 'commander';
import { intl } from '../intl.js';
import { assertGcpProjectConfigured, isInteractive, maybePromptForProjectId, withSpinner, } from './utils.js';
export const command = new Command('tail-logs')
    .alias('logs')
    .description('Print the most recent log entries')
    .option('--watch', 'Watch and print new logs')
    .option('--simplified', 'Hide timestamps with logs')
    .action(async function () {
    const options = this.optsWithGlobals();
    const clasp = options.clasp;
    const { json, simplified, watch } = options;
    const seenEntries = new Set();
    let since;
    const fetchAndPrintLogs = async () => {
        const spinnerMsg = intl.formatMessage({ id: "VtwoAE", defaultMessage: [{ type: 0, value: "Fetching logs..." }] });
        const entries = await withSpinner(spinnerMsg, async () => await clasp.logs.getLogEntries(since));
        entries.results.reverse().forEach(entry => {
            if (entry.timestamp) {
                since = new Date(entry.timestamp);
            }
            const id = entry.insertId;
            if (!id) {
                return;
            }
            if (seenEntries.has(id)) {
                return;
            }
            seenEntries.add(id);
            const msg = formatEntry(entry, {
                json,
                simplified,
            });
            if (msg) {
                console.log(msg);
            }
        });
    };
    if (!clasp.project.projectId && isInteractive()) {
        await maybePromptForProjectId(clasp);
    }
    assertGcpProjectConfigured(clasp);
    console.log('PAST', clasp.project.projectId);
    await fetchAndPrintLogs();
    if (watch) {
        const POLL_INTERVAL = 6000; // 6s
        setInterval(async () => {
            await fetchAndPrintLogs();
        }, POLL_INTERVAL);
    }
});
const severityColor = {
    ERROR: chalk.red,
    INFO: chalk.cyan,
    DEBUG: chalk.green, // Includes timeEnd
    NOTICE: chalk.magenta,
    WARNING: chalk.yellow,
};
function formatEntry(entry, options) {
    var _a, _b;
    const { severity = '', timestamp = '', resource } = entry;
    if (!resource) {
        return undefined;
    }
    if (!timestamp) {
        return undefined;
    }
    let functionName = (_b = (_a = resource.labels) === null || _a === void 0 ? void 0 : _a['function_name']) !== null && _b !== void 0 ? _b : 'N/A';
    let payloadData = '';
    if (options.json) {
        payloadData = JSON.stringify(entry, null, 2);
    }
    else {
        const { jsonPayload, textPayload } = entry;
        if (textPayload) {
            payloadData = textPayload;
        }
        else if (jsonPayload && jsonPayload.message) {
            payloadData = jsonPayload.message;
        }
        else if (jsonPayload) {
            payloadData = JSON.stringify(jsonPayload);
        }
        else {
            return undefined;
        }
    }
    const colorizer = severityColor[severity];
    const coloredSeverity = `${colorizer ? colorizer(severity) : severity}`.padEnd(20);
    functionName = functionName.padEnd(15);
    payloadData = payloadData.padEnd(20);
    const localizedTime = getLocalISODateTime(new Date(timestamp));
    if (options.simplified) {
        return `${coloredSeverity} ${functionName} ${payloadData}`;
    }
    return `${coloredSeverity} ${localizedTime} ${functionName} ${payloadData}`;
}
function getLocalISODateTime(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`;
}
