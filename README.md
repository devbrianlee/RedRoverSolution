# RedRover.Puzzle
- Run console application
- Type 'sorted' and press enter to toggle between sorted and unsorted outputs (Defaults to OFF)
- Enter valid NPCSV syntax to render it to a dash-bulleted, indented list
    - Example Input: 
        ```
        (id, name, email, type(id, name, customFields(c1, c2, c3)), externalId)
        ```
    - Example Output:
        ```
        - id
        - name
        - email
        - type
        - id
        - name
        - customFields
            - c1
            - c2
            - c3
        - externalId
        ```
- Note: Self-contained executable file available in releases/ directory