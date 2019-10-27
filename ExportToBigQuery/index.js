/**
 * Triggered from a message on a Cloud Pub/Sub topic.
 *
 * @param {!Object} event Event payload and metadata.
 * @param {!Function} callback Callback function to signal completion.
 */

const { google } = require('googleapis');
const { promisify } = require('util');
const {BigQuery} = require('@google-cloud/bigquery');
const bq = new BigQuery();

exports.helloPubSub = async (pubSubEvent, context) => {

  console.log("Received a message from pubsub: " + JSON.stringify(pubSubEvent));
              
  var data = Buffer.from(pubSubEvent.data, 'base64').toString();
  console.log("contains data: " + data);
  
  var record = JSON.parse(data);
  var record_date = Date.parse(record.date_time_utc);
  console.log("Data point: at " + record_date + ", temp_f: " + record.temp_f + ", temp_c: " + record.temp_c);

  const row = { date_time_utc: record.date_time_utc, 
                  device_id: record.device,
                  temp_f: record.temp_f, 
                  temp_c: record.temp_c };

  await bq.dataset("temperatures").table("datapoints").insert(row).then((data) => {
    // All rows inserted successfully
    var apiResponse = data[0];
  }).catch(err => {
    if (err.name === 'PartialFailureError') {
      // Insert partially, or entirelly failed
      console.log(JSON.stringify(err));
      //const insertErrors = err.insertErrors;
      //console.log(`Insert error: ${JSON.stringify(insertErrors[0])}`);
      throw err;
    } else {
      // 'err' could be a DNS error, a rate limit error, an auth error, etc.
      throw err;
    }
  });
  
  console.log(`Inserted 1 row`);
};