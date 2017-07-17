import csv
import os
from os import listdir
from os import path


def direction_axis_resolver(target_position_string):
    north_pos = east_pos = ["2", "5", "10"]
    south_pos = west_pos = ["-2", "-5", "-10"]
    direction_string = ""
    target_positions = target_position_string.split('_')

    # y axis
    if target_positions[1] in south_pos:
        direction_string += 'S'
    elif target_positions[1] in north_pos:
        direction_string += 'N'

    # x axis
    if target_positions[0] in west_pos:
        direction_string += 'W'
    elif target_positions[0] in east_pos:
        direction_string += 'E'

    return direction_string


# create a new file with new log structure
if not os.path.exists('output'):
    os.makedirs('output')
output_file = open('./output/data.csv', 'w')
fieldnames = ['user_id', 'trial_type', 'target_distance', 'direction_axis', 'nr_of_clutches', 'completion_time',
              'task_number']
output_csv_writer = csv.DictWriter(output_file, fieldnames=fieldnames)
output_csv_writer.writeheader()

output_csv_data = []
# TODO: using this for resetting the test_number
file_iterator = 1
for file_name in listdir(path="."):
    # do for each filename
    if file_name.endswith(".csv") and not path.isdir(file_name):
        # open file
        csv_file = open(file_name)
        # read csv in a dictionary
        csv_reader = csv.DictReader(csv_file, delimiter=',')
        # strip whitespaces from fieldnames
        csv_reader.fieldnames = [field.strip().lower() for field in csv_reader.fieldnames]

        # declaring output variables
        user_id = ""
        trial_type = ""
        distance = ""
        direction_axis = ""
        nr_of_clutches = 0
        completion_time = 0

        # declaring first timestamp variable
        first_timestamp = 0

        # first_row flag
        first_row = True

        timestamp = 0
        task_number = 0

        for row in csv_reader:
            # iterate row by row
            if first_row:
                # first row
                # setting up basic vars
                user_id = row['user_id']
                trial_type = row['trial_type']
                distance = row['target_distance']

                # figuring out which axis it belongs to
                direction_axis = direction_axis_resolver(row['target_start_pos'])

                # set start trial time
                first_timestamp = row['timestamp']

                # set task number
                task_number = row['task_number']

                first_row = False

            timestamp = row['timestamp']
            nr_of_clutches = row['nr_of_clutches']

        # finished iterating trough the entire file
        completion_time = float(timestamp) - float(first_timestamp)
        output_csv_writer.writerow({'user_id': user_id,
                                    'trial_type': trial_type,
                                    'target_distance': distance,
                                    'direction_axis': direction_axis,
                                    'nr_of_clutches': nr_of_clutches,
                                    'completion_time': completion_time,
                                    'task_number': task_number
                                    })
    else:
        continue
