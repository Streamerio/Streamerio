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
// This file defines the 'create-deployment' (alias 'deploy') command for the
// clasp CLI.
import { Command } from 'commander';
import { intl } from '../intl.js';
import { withSpinner } from './utils.js';
export const command = new Command('create-deployment')
    .alias('deploy')
    .description('Deploy a project')
    .option('-V, --versionNumber <version>', 'The project version')
    .option('-d, --description <description>', 'The deployment description')
    .option('-i, --deploymentId <id>', 'The deployment ID to redeploy')
    .action(async function () {
    var _a, _b, _c, _d, _e;
    const options = this.optsWithGlobals();
    const clasp = options.clasp;
    const deploymentId = options.deploymentId;
    const description = (_a = options.description) !== null && _a !== void 0 ? _a : '';
    const versionNumber = options.versionNumber ? Number(options.versionNumber) : undefined;
    try {
        const spinnerMsg = intl.formatMessage({ id: "oL8t7p", defaultMessage: [{ type: 0, value: "Deploying project..." }] });
        const deployment = await withSpinner(spinnerMsg, async () => {
            return clasp.project.deploy(description, deploymentId, versionNumber);
        });
        if (options.json) {
            const output = {
                deploymentId: deployment.deploymentId,
                versionNumber: (_b = deployment.deploymentConfig) === null || _b === void 0 ? void 0 : _b.versionNumber,
                description: (_c = deployment.deploymentConfig) === null || _c === void 0 ? void 0 : _c.description,
            };
            console.log(JSON.stringify(output, null, 2));
            return;
        }
        const successMessage = intl.formatMessage({ id: "182gSV", defaultMessage: [{ type: 0, value: "Deployed " }, { type: 1, value: "deploymentId" }, { type: 0, value: " " }, { type: 5, value: "version", options: { undefined: { value: [{ type: 0, value: "@HEAD" }] }, other: { value: [{ type: 0, value: "@" }, { type: 1, value: "version" }] } } }] }, {
            deploymentId: deployment.deploymentId,
            version: (_d = deployment.deploymentConfig) === null || _d === void 0 ? void 0 : _d.versionNumber,
        });
        console.log(successMessage);
    }
    catch (error) {
        if (((_e = error.cause) === null || _e === void 0 ? void 0 : _e.code) === 'INVALID_ARGUMENT') {
            this.error(error.cause.message);
        }
        throw error;
    }
});
